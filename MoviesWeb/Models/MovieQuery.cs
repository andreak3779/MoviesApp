namespace MoviesWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MovieQuery
    {
        private static readonly string[] AllowedKeys = { "title", "genre", "director", "releaseYear" };
        private readonly Dictionary<string, string> _queryParameters = new Dictionary<string, string>();

        public void Add(string key, string value)
        {
            if (!AllowedKeys.Contains(key))
            {
                throw new ArgumentException($"Invalid query key: {key}. Allowed keys are: {string.Join(", ", AllowedKeys)}");
            }
            _queryParameters[key] = value;
        }

        public IEnumerable<KeyValuePair<string, string>> AsEnumerable() => _queryParameters;

        public IEnumerable<T> Select<T>(Func<KeyValuePair<string, string>, T> selector) => _queryParameters.Select(selector);

        public IEnumerable<string> ToQueryStringParts() => _queryParameters.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");

        public bool Validate()
        {
            // Keys are validated on insertion in Add, so this method is a no-op that
            // exists for API compatibility and always indicates a valid state.
            return true;
        }
    }
}