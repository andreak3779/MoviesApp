using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MoviesApi.Models;

public class Movie
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }
    
    [Required]
    public int ReleaseYear { get; set; }
    
    // Navigation properties for many-to-many relationships
    public ICollection<MovieMediaType> MovieMediaTypes { get; set; } = new List<MovieMediaType>();
    public ICollection<MovieFilmGenre> MovieFilmGenres { get; set; } = new List<MovieFilmGenre>();
}