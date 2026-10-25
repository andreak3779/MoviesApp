using System.ComponentModel.DataAnnotations;

namespace MoviesApiDomain.Models;

/// <summary>
/// Represents a movie format category (e.g., Movie, TV Series, Box Set).
/// Distinct from <see cref="MediaType"/>, which describes the physical media
/// (DVD, Blu-Ray, etc.) on which a movie is stored.
/// </summary>
public class Format
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public required string Name { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    // Navigation property for the many-to-many relationship with movies.
    public ICollection<MovieFormat> MovieFormats { get; set; } = new List<MovieFormat>();
}
