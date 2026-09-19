using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApiDomain.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// The <c>jti</c> claim of the access token this refresh token was issued
    /// for. Used to bind a refresh token to the access-token family, so
    /// rotating the access token also invalidates outstanding refresh tokens.
    /// </summary>
    [MaxLength(64)]
    public string? AccessTokenJti { get; set; }

    /// <summary>
    /// When true, this refresh token has been used to mint a new pair and can
    /// never be used again. Single-use enforcement closes the replay window.
    /// </summary>
    public bool Consumed { get; set; }

    public DateTime? ConsumedAt { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
    
    public bool IsActive => !IsRevoked && !IsExpired;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
