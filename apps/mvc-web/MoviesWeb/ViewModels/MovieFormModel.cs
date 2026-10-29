namespace MoviesWeb.ViewModels;

using System.ComponentModel.DataAnnotations;
using MoviesWeb.Models;

/// <summary>
/// The add / edit movie form.
/// </summary>
/// <remarks>
/// The form edits one movie at a time, so it carries a single id per
/// lookup rather than the collections the API returns on a movie.
/// </remarks>
public class MovieFormModel
{
    /// <summary>Gets or sets the movie's identifier. Null when adding a movie.</summary>
    public Guid? Id { get; set; }

    /// <summary>Gets or sets the movie title.</summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the year of release.</summary>
    [Range(1888, 2100, ErrorMessage = "Year must be between 1888 and 2100.")]
    public int ReleaseYear { get; set; } = DateTime.UtcNow.Year;

    /// <summary>Gets or sets the plot summary.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a favourite.</summary>
    public bool Favourite { get; set; }

    /// <summary>Gets or sets a value indicating whether this is part of the Criterion Collection.</summary>
    public bool Criterion { get; set; }

    /// <summary>Gets or sets the Criterion Collection spine number.</summary>
    [Range(1, 9999, ErrorMessage = "Criterion number must be 1 or higher.")]
    public int? CriterionNumber { get; set; }

    /// <summary>Gets or sets the selected genre identifier.</summary>
    public int? FilmGenreId { get; set; }

    /// <summary>Gets or sets the selected media type identifier.</summary>
    public int? MediaTypeId { get; set; }

    /// <summary>Gets or sets the selected format identifier.</summary>
    public int? FormatId { get; set; }

    /// <summary>Gets or sets the genres the form offers.</summary>
    public List<FilmGenreDto> Genres { get; set; } = new();

    /// <summary>Gets or sets the media types the form offers.</summary>
    public List<MediaTypeDto> MediaTypes { get; set; } = new();

    /// <summary>Gets or sets the formats the form offers.</summary>
    public List<FormatDto> Formats { get; set; } = new();

    /// <summary>
    /// Builds a form model from an existing movie.
    /// </summary>
    /// <param name="movie">The movie to edit.</param>
    /// <returns>The populated form model.</returns>
    public static MovieFormModel FromMovie(Movie movie) => new()
    {
        Id = movie.Id,
        Title = movie.Title,
        ReleaseYear = movie.ReleaseYear,
        Description = movie.Description,
        Favourite = movie.Favourite,
        Criterion = movie.Criterion,
        CriterionNumber = movie.CriterionNumber,
        FilmGenreId = movie.FilmGenres.FirstOrDefault()?.Id,
        MediaTypeId = movie.MediaTypes.FirstOrDefault()?.Id,
        FormatId = movie.Formats.FirstOrDefault()?.Id
    };

    /// <summary>
    /// Builds the create payload for the Web API.
    /// </summary>
    /// <returns>The create request.</returns>
    public MovieCreateRequest ToCreateRequest() => new()
    {
        Title = Title.Trim(),
        ReleaseYear = ReleaseYear,
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Favourite = Favourite,
        Criterion = Criterion,
        CriterionNumber = Criterion ? CriterionNumber : null,
        FilmGenreId = FilmGenreId,
        MediaTypeId = MediaTypeId,
        FormatId = FormatId
    };

    /// <summary>
    /// Builds the update payload for the Web API.
    /// </summary>
    /// <returns>The update request.</returns>
    public MovieUpdateRequest ToUpdateRequest() => new()
    {
        Title = Title.Trim(),
        ReleaseYear = ReleaseYear,
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Favourite = Favourite,
        Criterion = Criterion,
        CriterionNumber = Criterion ? CriterionNumber : null,
        FilmGenreId = FilmGenreId,
        MediaTypeId = MediaTypeId,
        FormatId = FormatId
    };
}
