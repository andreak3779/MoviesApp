using System.Net;
using System.Net.Http.Json;
using MoviesApi.DTOs;
using MoviesApi.Tests.Infrastructure;
using Xunit;

namespace MoviesApi.Tests.Integration;

/// <summary>
/// The token lifecycle is the security boundary of the API: a refresh token
/// may only be used once, only by the account it was issued to, and only
/// alongside the access token it was minted with.
///
/// These tests drive the real endpoints so the rules are pinned where the
/// application enforces them, not only in the service.
/// </summary>
public class AuthTokenIntegrationTests : IClassFixture<MoviesApiFactory>
{
    private const string Password = "Passw0rd!2026";

    private readonly MoviesApiFactory _factory;

    public AuthTokenIntegrationTests(MoviesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Refresh_WithAValidPair_ReturnsANewPair()
    {
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = auth.Token, refreshToken = auth.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(refreshed);
        Assert.False(string.IsNullOrWhiteSpace(refreshed!.Token));
        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        Assert.NotEqual(auth.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task Refresh_TokenIsSingleUse()
    {
        // A refresh token that could be replayed would let a stolen token
        // mint access tokens forever. The first use consumes it; the second
        // must fail.
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);

        var first = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = auth.Token, refreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = auth.Token, refreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithADifferentSessionToken_IsRejected()
    {
        // Signing in again produces a second access token. The outstanding
        // refresh token is bound to the access token it was issued with, so
        // pairing it with the newer one is treated as suspicious.
        var client = _factory.CreateClient();
        var first = await RegisterAsync(client);
        var second = await LoginAsync(client, first.Email);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = second.Token, refreshToken = first.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithAnotherUsersRefreshToken_IsRejected()
    {
        var client = _factory.CreateClient();
        var owner = await RegisterAsync(client);
        var other = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = other.Token, refreshToken = owner.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithAGarbageAccessToken_IsRejected()
    {
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = "not-a-jwt", refreshToken = auth.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithAGarbageRefreshToken_IsRejected()
    {
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = auth.Token, refreshToken = "not-a-refresh-token" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithoutABody_IsRejected()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/refresh", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesTheRefreshToken()
    {
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);

        var logout = await client.PostAsJsonAsync(
            "/api/v1/Auth/logout",
            new { token = auth.Token, refreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        // The revoked token can no longer be exchanged for an access token.
        var refresh = await client.PostAsJsonAsync(
            "/api/v1/Auth/refresh",
            new { token = auth.Token, refreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Logout_WithAnUnknownRefreshToken_IsBadRequest()
    {
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/logout",
            new { token = auth.Token, refreshToken = "not-a-refresh-token" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAToken_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/logout",
            new { token = "token", refreshToken = "refresh" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsTheCaller()
    {
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.GetAsync("/api/v1/Auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.Equal(auth.Email, me!.Email);
        Assert.Contains("User", me.Roles);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAToken_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/Auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithAnExistingEmail_IsRejected()
    {
        // The error message must not say whether the email is taken, or the
        // endpoint becomes an account-enumeration oracle.
        var client = _factory.CreateClient();
        var auth = await RegisterAsync(client);
        var email = auth.Email;

        var response = await client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            email,
            password = Password,
            confirmPassword = Password,
            firstName = "Second",
            lastName = "Try"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(body.Contains("exists", StringComparison.OrdinalIgnoreCase));
        Assert.False(body.Contains(email, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Register_WithAMismatchedPassword_IsRejected()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            email = $"mismatch-{Guid.NewGuid():N}@moviesapp.test",
            password = Password,
            confirmPassword = "Different!2026",
            firstName = "Mismatch",
            lastName = "Tester"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ForAnUnknownEmail_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { email = $"nobody-{Guid.NewGuid():N}@moviesapp.test", password = Password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---- helpers -------------------------------------------------------

    private static async Task<AuthResponseDto> RegisterAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            email = $"auth-{Guid.NewGuid():N}@moviesapp.test",
            password = Password,
            confirmPassword = Password,
            firstName = "Token",
            lastName = "Tester"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!;
    }

    private static async Task<AuthResponseDto> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { email, password = Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!;
    }

    private sealed class MeResponse
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
