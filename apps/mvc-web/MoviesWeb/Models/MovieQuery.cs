namespace MoviesWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Builds the query string sent to <c>GET /api/v1/Movie</c>.
    /// </summary>
    /// <remarks>
    /// The allowed keys mirror the API's <c>MovieQuery</c> exactly:
    /// <c>search</c>, <c>genre</c>, <c>mediaType</c>, <c>format</c>,
    /// <c>sortBy</c>, <c>sortDir</c>, <c>pageNumber</c>, <c>pageSize</c>.
    /// Keys are validated on insertion so a caller cannot smuggle arbitrary
    /// query parameters into the upstream request.
    /// </remarks>
    public class MovieQuery
    {
        private static readonly string[] AllowedKeys =
        {
            "search", "genre", "mediaType", "format",
            "sortBy", "sortDir", "pageNumber", "pageSize"
        };
        private readonly Dictionary<string, string> _queryParameters = new Dictionary<string, string>();

        /// <summary>
        /// Adds or replaces a query parameter.
        /// </summary>
        /// <param name="key">The parameter name. Must be one of the allowed keys.</param>
        /// <param name="value">The parameter value. Null or blank values are ignored.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is not allowed.</exception>
        public void Add(string key, string? value)
        {
            if (!AllowedKeys.Contains(key))
            {
                throw new ArgumentException($"Invalid query key: {key}. Allowed keys are: {string.Join(", ", AllowedKeys)}");
            }

            // Blank values are dropped so the API applies its own defaults
            // rather than receiving an empty filter.
            if (string.IsNullOrWhiteSpace(value))
            {
                _queryParameters.Remove(key);
                return;
            }

            _queryParameters[key] = value;
        }

        /// <summary>Gets the query parameters as key/value pairs.</summary>
        public IEnumerable<KeyValuePair<string, string>> AsEnumerable() => _queryParameters;

        /// <summary>Projects the query parameters using the supplied selector.</summary>
        public IEnumerable<T> Select<T>(Func<KeyValuePair<string, string>, T> selector) => _queryParameters.Select(selector);

        /// <summary>Gets the escaped query string, without a leading <c>?</c>.</summary>
        public IEnumerable<string> ToQueryStringParts() => _queryParameters.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");

        /// <summary>
        /// Validates the query. Keys are validated on insertion, so this is a
        /// no-op that exists for API compatibility and always indicates a valid state.
        /// </summary>
        /// <returns>Always <c>true</c>.</returns>
        public bool Validate()
        {
            return true;
        }
    }
}