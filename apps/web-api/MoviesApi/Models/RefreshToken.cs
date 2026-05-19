using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApi.Models;

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
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
    
    public bool IsActive => !IsRevoked && !IsExpired;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
