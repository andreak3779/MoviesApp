namespace MoviesWeb.ViewModels;

using MoviesWeb.Models;

/// <summary>
/// Represents the data required to render a list of movies, including paging metadata.
/// </summary>
public class MovieListModel
{
    /// <summary>
    /// Gets or sets the list of movies to display on the current page.
    /// </summary>
    public List<Movie> Movies { get; set; } = new();

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the size of each page (number of items per page).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items available across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page of results.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page of results.
    /// </summary>
    public bool HasNextPage => PageSize > 0 && (long)PageNumber * PageSize < TotalCount;

    /// <summary>
    /// Gets or sets the genres available for the filter dropdown.
    /// </summary>
    public List<FilmGenreDto> Genres { get; set; } = new();

    /// <summary>
    /// Gets or sets the media types available for the filter dropdown.
    /// </summary>
    public List<MediaTypeDto> MediaTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the formats available for the filter dropdown.
    /// </summary>
    public List<FormatDto> Formats { get; set; } = new();

    /// <summary>
    /// Gets or sets the currently applied free-text search term.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets the currently applied genre filter.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Gets or sets the currently applied media type filter.
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// Gets or sets the currently applied format filter.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the active sort key (<c>title</c> or <c>year</c>).
    /// </summary>
    public string SortBy { get; set; } = "year";

    /// <summary>
    /// Gets or sets the active sort direction (<c>asc</c> or <c>desc</c>).
    /// </summary>
    public string SortDir { get; set; } = "desc";
}
