using MoviesApi.Models;

namespace MoviesApi.DTOs;

/// <summary>
/// DTO for creating/updating a movie with related media types and film genres
/// </summary>
public class MovieDto
{
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public List<int> MediaTypeIds { get; set; } = new();
    public List<int> FilmGenreIds { get; set; } = new();
}

/// <summary>
/// DTO for returning movie data with expanded related entities
/// </summary>
public class MovieResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
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
