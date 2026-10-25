namespace MoviesApiDomain.Models;

/// <summary>
/// Query parameters for the movies list endpoint. Lives in Domain so the
/// repository can apply it without taking a transport-layer dependency.
/// </summary>
public class MovieQuery
{
    public string? Search { get; set; }
    public string? Genre { get; set; }
    public string? MediaType { get; set; }
    public string? Format { get; set; }

    /// <summary>"title" or "year"; defaults to "year".</summary>
    public string SortBy { get; set; } = "year";

    /// <summary>"asc" or "desc"; defaults to "desc" for year, "asc" for title.</summary>
    public string SortDir { get; set; } = "desc";

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
