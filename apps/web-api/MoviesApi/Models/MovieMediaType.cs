using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApi.Models;

/// <summary>
/// Junction table for many-to-many relationship between Movie and MediaType
/// </summary>
public class MovieMediaType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey(nameof(Movie))]
    public int MovieId { get; set; }
    
    [Required]
    [ForeignKey(nameof(MediaType))]
    public int MediaTypeId { get; set; }
    
    // Navigation properties
    public Movie Movie { get; set; } = null!;
    public MediaType MediaType { get; set; } = null!;
}
