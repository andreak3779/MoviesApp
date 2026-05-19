using System.ComponentModel.DataAnnotations;
using MoviesApi.Models;

namespace MoviesApi.DTOs;

/// <summary>
/// DTO for creating a new movie
/// </summary>
public class MovieCreateDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Range(1888, 2100)]
    public int ReleaseYear { get; set; }
    
    public List<int> MediaTypeIds { get; set; } = new();
    public List<int> FilmGenreIds { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing movie (all properties optional)
/// </summary>
public class MovieUpdateDto
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }
    
    [Range(1888, 2100)]
    public int? ReleaseYear { get; set; }
    
    public List<int>? MediaTypeIds { get; set; }
    public List<int>? FilmGenreIds { get; set; }
}

/// <summary>
/// DTO for returning movie data with expanded related entities
/// </summary>
public class MovieResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<MediaTypeDto> MediaTypes { get; set; } = new();
    public List<FilmGenreDto> FilmGenres { get; set; } = new();
}

/// <summary>
/// Simple DTO for MediaType
/// </summary>
public class MediaTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Simple DTO for FilmGenre
/// </summary>
public class FilmGenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
