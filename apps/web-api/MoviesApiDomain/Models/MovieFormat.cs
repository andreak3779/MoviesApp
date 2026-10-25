using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApiDomain.Models;

/// <summary>
/// Junction table for the many-to-many relationship between Movie and Format.
/// Mirrors <see cref="MovieMediaType"/>.
/// </summary>
public class MovieFormat
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Movie))]
    public Guid MovieId { get; set; }

    [Required]
    [ForeignKey(nameof(Format))]
    public int FormatId { get; set; }

    public Movie Movie { get; set; } = null!;
    public Format Format { get; set; } = null!;
}
