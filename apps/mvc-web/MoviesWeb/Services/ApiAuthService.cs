namespace MoviesWeb.Services
{
    using System.Net.Http.Json;
    using System.Text.Json;
    using MoviesWeb.Models;

    /// <summary>
    /// Authenticates against the Web API's auth endpoints.
    /// </summary>
    public interface IApiAuthService
    {
        /// <summary>Signs in and returns the issued tokens.</summary>
        Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        /// <summary>Registers a new account and returns the issued tokens.</summary>
        Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

        /// <summary>Exchanges a refresh token for a fresh access token.</summary>
        Task<AuthResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);

        /// <summary>Revokes the refresh token server-side.</summary>
        /// <param name="token">The caller's access token. The API authorises the request with it.</param>
        /// <param name="refreshToken">The refresh token to revoke.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns><c>true</c> when the API revoked the token.</returns>
        Task<bool> LogoutAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// HTTP client for the Web API's <c>/api/v1/auth</c> endpoints.
    /// </summary>
    public class ApiAuthService : IApiAuthService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiAuthService"/> class.
        /// </summary>
        /// <param name="httpClient">Client configured with the API base address.</param>
        public ApiAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "Auth/login", request, ApiJson.Options, cancellationToken);

            return await ReadAsync(response, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "Auth/register", request, ApiJson.Options, cancellationToken);

            return await ReadAsync(response, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AuthResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "Auth/refresh", request, ApiJson.Options, cancellationToken);

            return await ReadAsync(response, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> LogoutAsync(
            string token,
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            // The API authorises the logout with the access token and revokes
            // the refresh token, so both have to be sent.
            var response = await _httpClient.PostAsJsonAsync(
                "Auth/logout",
                new RefreshRequest { Token = token ?? string.Empty, RefreshToken = refreshToken },
                ApiJson.Options,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }

        private static async Task<AuthResponse?> ReadAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<AuthResponse>(body, ApiJson.Options);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
