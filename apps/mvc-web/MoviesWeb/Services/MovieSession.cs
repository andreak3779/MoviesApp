namespace MoviesWeb.Services
{
    using System.Globalization;
    using MoviesWeb.Models;

    /// <summary>
    /// Holds the signed-in user's API tokens for the duration of the session.
    /// </summary>
    public interface IMovieSession
    {
        /// <summary>Gets a value indicating whether the user is signed in.</summary>
        bool IsSignedIn { get; }

        /// <summary>Gets the signed-in user's email address, if known.</summary>
        string? Email { get; }

        /// <summary>Gets the current access token, if any.</summary>
        string? AccessToken { get; }

        /// <summary>Gets the current refresh token, if any.</summary>
        /// <remarks>
        /// Signing out revokes this token server-side, so it has to be
        /// readable by the sign-out path.
        /// </remarks>
        string? RefreshToken { get; }

        /// <summary>Stores the tokens returned by a successful sign-in.</summary>
        void SignIn(AuthResponse auth);

        /// <summary>Clears the stored tokens.</summary>
        void SignOut();

        /// <summary>
        /// Returns a valid access token, refreshing it when the current one has expired.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The access token, or <c>null</c> when the session cannot be refreshed.</returns>
        Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Session-backed implementation of <see cref="IMovieSession"/>.
    /// </summary>
    /// <remarks>
    /// The access token is short-lived (15 minutes by default), so the session
    /// refreshes it transparently using the stored refresh token. Refresh
    /// tokens are single-use on the API, so each refresh stores the new pair.
    /// </remarks>
    public class MovieSession : IMovieSession
    {
        // Refresh this long before the token actually expires so in-flight
        // requests do not race the expiry.
        private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(1);

        /// <summary>
        /// The session key under which the access token is stored.
        /// </summary>
        public const string AccessTokenKey = "MoviesApi.AccessToken";

        /// <summary>
        /// The session key under which the refresh token is stored.
        /// </summary>
        public const string RefreshTokenKey = "MoviesApi.RefreshToken";

        private const string ExpiresAtKey = "MoviesApi.ExpiresAt";
        private const string EmailKey = "MoviesApi.Email";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiAuthService _authService;
        private readonly ILogger<MovieSession> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovieSession"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Accessor used to read and write the session.</param>
        /// <param name="authService">Service used to refresh expired tokens.</param>
        /// <param name="logger">Logger for refresh failures.</param>
        public MovieSession(
            IHttpContextAccessor httpContextAccessor,
            IApiAuthService authService,
            ILogger<MovieSession> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
            _logger = logger;
        }

        /// <inheritdoc />
        public bool IsSignedIn => !string.IsNullOrEmpty(GetSession()?.GetString(AccessTokenKey));

        /// <inheritdoc />
        public string? Email => GetSession()?.GetString(EmailKey);

        /// <inheritdoc />
        public string? AccessToken => GetSession()?.GetString(AccessTokenKey);

        /// <inheritdoc />
        public string? RefreshToken => GetSession()?.GetString(RefreshTokenKey);

        /// <inheritdoc />
        public void SignIn(AuthResponse auth)
        {
            var session = GetSession();
            if (session is null)
            {
                return;
            }

            session.SetString(AccessTokenKey, auth.Token);
            session.SetString(RefreshTokenKey, auth.RefreshToken);
            session.SetString(ExpiresAtKey, auth.ExpiresAt.ToString("O", CultureInfo.InvariantCulture));
            session.SetString(EmailKey, auth.Email);
        }

        /// <inheritdoc />
        public void SignOut()
        {
            var session = GetSession();
            session?.Remove(AccessTokenKey);
            session?.Remove(RefreshTokenKey);
            session?.Remove(ExpiresAtKey);
            session?.Remove(EmailKey);
        }

        /// <inheritdoc />
        public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var session = GetSession();
            if (session is null)
            {
                return null;
            }

            var token = session.GetString(AccessTokenKey);
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            if (!NeedsRefresh(session))
            {
                return token;
            }

            var refreshToken = session.GetString(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                SignOut();
                return null;
            }

            try
            {
                var refreshed = await _authService.RefreshAsync(
                    new RefreshRequest { Token = token, RefreshToken = refreshToken },
                    cancellationToken);

                if (refreshed is null)
                {
                    _logger.LogWarning("Token refresh failed; signing the user out.");
                    SignOut();
                    return null;
                }

                SignIn(refreshed);
                return refreshed.Token;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Token refresh request failed; signing the user out.");
                SignOut();
                return null;
            }
        }

        private static bool NeedsRefresh(ISession session)
        {
            var expiresAtRaw = session.GetString(ExpiresAtKey);
            if (string.IsNullOrEmpty(expiresAtRaw)
                || !DateTime.TryParse(
                    expiresAtRaw,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var expiresAt))
            {
                // Unknown expiry - do not force a refresh on every request.
                return false;
            }

            var effective = expiresAt.Kind == DateTimeKind.Utc ? expiresAt : expiresAt.ToUniversalTime();
            return effective - DateTime.UtcNow <= RefreshMargin;
        }

        private ISession? GetSession() => _httpContextAccessor.HttpContext?.Session;
    }
}
