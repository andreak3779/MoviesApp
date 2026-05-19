using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Data;
using MoviesApi.DTOs;
using MoviesApi.Models;

namespace MoviesApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> RefreshTokenAsync(string token, string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly MoviesDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        MoviesDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return null;
        }

        // Assign default user role
        await _userManager.AddToRoleAsync(user, "User");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null)
        {
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return null;
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string token, string refreshToken)
    {
        try
        {
            // Validate the access token
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return null;
            }

            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return null;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            // Find and validate refresh token
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == user.Id);

            if (storedToken == null || !storedToken.IsActive)
            {
                return null;
            }

            // Revoke old token
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate new tokens
            return await GenerateAuthResponseAsync(user);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null || !token.IsActive)
        {
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var jwtToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7)),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            RefreshToken = refreshToken,
            ExpiresAt = jwtToken.ValidTo,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    private JwtSecurityToken GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "MoviesApi";
        var audience = jwtSettings["Audience"] ?? "MoviesAppUsers";
        var expirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes", 15);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"?.Trim()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add user roles
        var roles = _userManager.GetRolesAsync(user).Result;
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // We expect the token to be expired
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
