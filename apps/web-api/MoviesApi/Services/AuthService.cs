using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoviesApiData.Data;
using MoviesApi.DTOs;
using MoviesApiDomain.Models;

namespace MoviesApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> RefreshTokenAsync(string token, string refreshToken);

    /// <summary>
    /// Revokes a refresh token. The userId parameter is the caller's
    /// authenticated user id and is used to scope the lookup so a user
    /// cannot revoke another user's refresh token.
    /// </summary>
    Task<bool> RevokeTokenAsync(string userId, string refreshToken);
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

            // The refresh token must belong to *this* user and bind to the
            // same jti family as the access token the caller presented. A
            // stolen refresh token alone is no longer sufficient.
            var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == user.Id);

            if (storedToken == null || !storedToken.IsActive)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(storedToken.AccessTokenJti) &&
                !string.Equals(storedToken.AccessTokenJti, jti, StringComparison.Ordinal))
            {
                // Suspicious: caller is presenting a refresh token whose
                // jti family doesn't match the access token. Reject.
                return null;
            }

            // Atomic single-use revoke: in one round-trip, refuse to read
            // the token again if it has already been consumed. Avoids the
            // race where two concurrent refreshes both succeed.
            var now = DateTime.UtcNow;
            var rows = await _context.RefreshTokens
                .Where(rt => rt.Id == storedToken.Id && !rt.Consumed && rt.RevokedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(rt => rt.Consumed, true)
                    .SetProperty(rt => rt.ConsumedAt, now)
                    .SetProperty(rt => rt.RevokedAt, now));

            if (rows == 0)
            {
                // Already consumed — treat as replay and refuse.
                return null;
            }

            // Generate new tokens. Note: the new refresh token carries the
            // jti of the *new* access token, so the next refresh must
            // present a token from the same family.
            var response = await GenerateAuthResponseAsync(user);
            if (response is not null && jti is not null)
            {
                // Persist the new refresh token's jti binding.
                var newRefresh = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == response.RefreshToken);
                if (newRefresh is not null)
                {
                    newRefresh.AccessTokenJti = ExtractJtiFromJwt(response.Token);
                }
                await _context.SaveChangesAsync();
            }

            return response;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string userId, string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

        if (token == null || !token.IsActive)
        {
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        token.Consumed = true;
        token.ConsumedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private static string? ExtractJtiFromJwt(string token)
    {
        try
        {
            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return parsed.Id;
        }
        catch
        {
            return null;
        }
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var jwtToken = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var jti = jwtToken.Id.ToString();

        // Store refresh token, bound to the new access token's jti family.
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            AccessTokenJti = jti,
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

    private async Task<JwtSecurityToken> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        EnsureSecretKeyIsValid(secretKey);
        var issuer = jwtSettings["Issuer"] ?? "MoviesApi";
        var audience = jwtSettings["Audience"] ?? "MoviesAppUsers";
        var expirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes", 15);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add user roles
        var roles = await _userManager.GetRolesAsync(user);
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

    private static void EnsureSecretKeyIsValid(string secretKey)
    {
        const string PlaceholderJwtKey =
            "CHANGE_THIS_TO_A_SECURE_RANDOM_STRING_MIN_32_CHARS_IN_PRODUCTION";
        if (string.Equals(secretKey, PlaceholderJwtKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "JwtSettings:SecretKey is the placeholder value; configure a real " +
                "secret before starting the API.");
        }
        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:SecretKey must be at least 32 bytes (UTF-8).");
        }
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        EnsureSecretKeyIsValid(secretKey);

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
