using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApi.Models;

/// <summary>
/// Junction table for many-to-many relationship between Movie and FilmGenre
/// </summary>
public class MovieFilmGenre
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey(nameof(Movie))]
    public Guid MovieId { get; set; }
    
    [Required]
    [ForeignKey(nameof(FilmGenre))]
    public int FilmGenreId { get; set; }
    
    // Navigation properties
    public Movie Movie { get; set; } = null!;
    public FilmGenre FilmGenre { get; set; } = null!;
}
