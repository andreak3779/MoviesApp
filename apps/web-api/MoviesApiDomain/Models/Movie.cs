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

    /// <summary>Plain-text description / plot summary shown on cards.</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Whether this entry is part of the user's favourites list.</summary>
    public bool Favourite { get; set; }

    /// <summary>Whether this entry is part of the Criterion Collection.</summary>
    public bool Criterion { get; set; }

    /// <summary>Criterion Collection spine number; null when not Criterion.</summary>
    public int? CriterionNumber { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for many-to-many relationships
    public ICollection<MovieMediaType> MovieMediaTypes { get; set; } = new List<MovieMediaType>();
    public ICollection<MovieFilmGenre> MovieFilmGenres { get; set; } = new List<MovieFilmGenre>();
    public ICollection<MovieFormat> MovieFormats { get; set; } = new List<MovieFormat>();
}