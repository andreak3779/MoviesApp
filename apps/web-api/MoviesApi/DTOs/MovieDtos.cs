using System.ComponentModel.DataAnnotations;
using MoviesApiDomain.Models;

namespace MoviesApi.DTOs;

/// <summary>
/// DTO for creating a new movie. Mockup-shaped: single-select for
/// Genre / Format / MediaType. The full list-of-ids is still accepted for
/// backwards compatibility with the existing API contract.
/// </summary>
public class MovieCreateDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Range(1888, 2100)]
    public int ReleaseYear { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public bool Favourite { get; set; }

    public bool Criterion { get; set; }

    public int? CriterionNumber { get; set; }

    /// <summary>Single-select on the mockup form.</summary>
    public int? FilmGenreId { get; set; }

    /// <summary>Single-select on the mockup form.</summary>
    public int? MediaTypeId { get; set; }

    /// <summary>Single-select on the mockup form.</summary>
    public int? FormatId { get; set; }

    /// <summary>Legacy many-to-many inputs; ignored when the single-id fields are set.</summary>
    public List<int> MediaTypeIds { get; set; } = new();
    public List<int> FilmGenreIds { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing movie. All properties optional.
/// </summary>
public class MovieUpdateDto
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    [Range(1888, 2100)]
    public int? ReleaseYear { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public bool? Favourite { get; set; }

    public bool? Criterion { get; set; }

    public int? CriterionNumber { get; set; }

    public int? FilmGenreId { get; set; }
    public int? MediaTypeId { get; set; }
    public int? FormatId { get; set; }

    public List<int>? MediaTypeIds { get; set; }
    public List<int>? FilmGenreIds { get; set; }
}

/// <summary>
/// DTO returned by the API for a movie. Carries the denormalised single-value
/// fields the mockup renders directly (Genre / Format / MediaType) plus the
/// full collections in case the UI wants them.
/// </summary>
public class MovieResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string? Description { get; set; }
    public bool Favourite { get; set; }
    public bool Criterion { get; set; }
    public int? CriterionNumber { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Denormalised single-value fields for the mockup.
    public string? Genre { get; set; }
    public string? MediaType { get; set; }
    public string? Format { get; set; }

    // Full collections for callers that want them.
    public List<MediaTypeDto> MediaTypes { get; set; } = new();
    public List<FilmGenreDto> FilmGenres { get; set; } = new();
    public List<FormatDto> Formats { get; set; } = new();
}

/// <summary>
/// Query parameters for GET /api/v1/movies. The class lives in
/// MoviesApiDomain.Models so the repository can use it without a transport
/// dependency; the controller binds to it directly from the query string.
/// </summary>
// (See MoviesApiDomain.Models.MovieQuery)

public class MediaTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class FilmGenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class FormatDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
