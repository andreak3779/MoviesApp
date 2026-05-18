using System.ComponentModel.DataAnnotations;

namespace MoviesApi.Models;

/// <summary>
/// Represents a film genre category (e.g., Action, Drama, Sci-Fi)
/// </summary>
public class FilmGenre
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    // Navigation property for many-to-many relationship
    public ICollection<MovieFilmGenre> MovieFilmGenres { get; set; } = new List<MovieFilmGenre>();
}
