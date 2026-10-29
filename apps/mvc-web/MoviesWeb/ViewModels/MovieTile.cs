namespace MoviesWeb.ViewModels;

/// <summary>
/// The poster tile the mockup puts on every movie card: a coloured square
/// with the movie's initial on it.
/// </summary>
/// <remarks>
/// The colour is derived from the movie's identifier, so the same movie
/// always gets the same tile and the grid reads as a collection rather
/// than a list of identical cards. The Angular client derives its tiles
/// the same way.
/// </remarks>
public static class MovieTile
{
    private static readonly string[] Colors =
    {
        "oklch(40% 0.19 350)",
        "oklch(42% 0.17 320)",
        "oklch(44% 0.15 295)",
        "oklch(46% 0.14 270)",
        "oklch(43% 0.16 10)"
    };

    /// <summary>Gets the background colour of the tile for a movie.</summary>
    /// <param name="id">The movie's identifier.</param>
    /// <returns>A CSS colour.</returns>
    public static string Color(Guid id)
    {
        unchecked
        {
            var hash = 0;
            var text = id.ToString();
            for (var i = 0; i < text.Length; i++)
            {
                hash = (hash * 31) + text[i];
            }

            return Colors[Math.Abs(hash) % Colors.Length];
        }
    }

    /// <summary>Gets the letter shown on the tile for a movie.</summary>
    /// <param name="title">The movie's title.</param>
    /// <returns>The first letter of the title, upper-cased.</returns>
    public static string Letter(string? title)
    {
        var trimmed = title?.Trim();
        return string.IsNullOrEmpty(trimmed) ? "?" : trimmed[0].ToString().ToUpperInvariant();
    }
}
