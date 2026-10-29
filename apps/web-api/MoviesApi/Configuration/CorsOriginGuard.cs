namespace MoviesApi.Configuration;

/// <summary>
/// Validates the configured CORS origins before the API starts serving.
/// <para>
/// In Production the API refuses to start when an origin is still one of the
/// placeholder values the repository ships with, because
/// <c>AllowCredentials()</c> makes an over-permissive origin list genuinely
/// dangerous. The check compares the whole origin rather than a prefix: the
/// previous prefix match on <c>http://localhost</c> also rejected the
/// legitimate development origins (<c>http://localhost:4200</c>,
/// <c>http://localhost:4000</c>, <c>http://localhost:8080</c>) that ship in
/// <c>appsettings.json</c> and that the container stack needs, which stopped
/// the stack from starting at all.
/// </para>
/// </summary>
public static class CorsOriginGuard
{
    /// <summary>
    /// The placeholder origins the repository ships with. Each is matched in
    /// full; a longer origin that merely starts with the same text is a real
    /// origin and is allowed.
    /// </summary>
    private static readonly string[] PlaceholderOrigins =
    {
        "https://your-production-domain.com",
        "http://localhost",
    };

    /// <summary>
    /// Returns the placeholder origin that <paramref name="origin"/> matches,
    /// or <c>null</c> when the origin is acceptable.
    /// </summary>
    public static string? FindPlaceholder(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return null;

        var trimmed = origin.Trim();

        foreach (var placeholder in PlaceholderOrigins)
        {
            if (string.Equals(trimmed, placeholder, StringComparison.OrdinalIgnoreCase))
                return placeholder;
        }

        return null;
    }

    /// <summary>
    /// Throws when any of <paramref name="origins"/> is still a placeholder.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// At least one origin is a placeholder value.
    /// </exception>
    public static void EnsureNoPlaceholders(IEnumerable<string> origins)
    {
        foreach (var origin in origins)
        {
            var placeholder = FindPlaceholder(origin);
            if (placeholder != null)
            {
                throw new InvalidOperationException(
                    $"Cors:AllowedOrigins contains the placeholder value '{origin}'. " +
                    "Configure real origins before starting the API in Production.");
            }
        }
    }
}