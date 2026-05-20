using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace MoviesApiDomain.Models;

public class Movie
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }
    
    [Required]
    public int ReleaseYear { get; set; }
    
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser? Owner { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties for many-to-many relationships
    public ICollection<MovieMediaType> MovieMediaTypes { get; set; } = new List<MovieMediaType>();
    public ICollection<MovieFilmGenre> MovieFilmGenres { get; set; } = new List<MovieFilmGenre>();
}