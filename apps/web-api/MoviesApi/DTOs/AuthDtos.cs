using System.ComponentModel.DataAnnotations;

namespace MoviesApi.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class RefreshTokenDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
