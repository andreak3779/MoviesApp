namespace MoviesWeb.Services
{
    using System.Net.Http.Headers;
    using System.Text.Json;

    /// <summary>
    /// Adds the Web API's bearer token to outgoing requests.
    /// </summary>
    /// <remarks>
    /// The token is read from the current HTTP context's session on every
    /// request, so a refreshed token takes effect without restarting the app.
    /// Requests to the auth endpoints are left untouched — sending a stale
    /// bearer token to <c>/auth/login</c> would poison the login flow.
    /// </remarks>
    public sealed class ApiTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTokenHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Accessor used to read the caller's session.</param>
        /// <param name="serviceProvider">
        /// Used to resolve the session lazily. Resolving it in the constructor
        /// would create a DI cycle: the session refreshes tokens through an
        /// API client, which in turn needs this handler.
        /// </param>
        public ApiTokenHandler(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!IsAuthEndpoint(request.RequestUri))
            {
                var token = await GetAccessTokenAsync(cancellationToken);
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Reads the current access token from the session, refreshing it when needed.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The bearer token, or <c>null</c> when the user is not signed in.</returns>
        private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session is null)
            {
                return null;
            }

            var movieSession = _serviceProvider.GetService<IMovieSession>();
            return movieSession is null
                ? session.GetString(MovieSession.AccessTokenKey)
                : await movieSession.GetAccessTokenAsync(cancellationToken);
        }

        private static bool IsAuthEndpoint(Uri? requestUri)
        {
            if (requestUri is null)
            {
                return false;
            }

            var path = requestUri.AbsolutePath;
            return path.EndsWith("/auth/login", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("/auth/register", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("/auth/refresh", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Shared JSON options for talking to the Web API.
    /// </summary>
    /// <remarks>
    /// The API serialises in camelCase and binds query strings and bodies
    /// case-insensitively, so camelCase is used on the wire throughout.
    /// </remarks>
    public static class ApiJson
    {
        /// <summary>The JSON options used for all API requests and responses.</summary>
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
