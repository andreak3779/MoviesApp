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
    [StringLength(100)]
    public required string FilmGenre { get; set; }
    
    [Required]
    public int ReleaseYear { get; set; }
}