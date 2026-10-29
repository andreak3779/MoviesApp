using System.Text;
using System.Text.Json;
using MoviesWeb.Models;
using MoviesWeb.Services;

namespace MoviesWeb.Tests.Services;

/// <summary>
/// Verifies that the auth client talks to the Web API's auth endpoints using
/// the routes and payload shapes the API actually exposes.
/// </summary>
public class ApiAuthServiceTests
{
    private const string BaseUrl = "http://api.test/api/v1/";

    private readonly Mock<HttpMessageHandler> _handler = new(MockBehavior.Strict);

    private readonly List<HttpRequestMessage> _requests = new();

    private ApiAuthService CreateService() =>
        new(new HttpClient(_handler.Object) { BaseAddress = new Uri(BaseUrl) });

    private void Respond(HttpStatusCode status, string json)
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => _requests.Add(request))
            .ReturnsAsync(() => new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }

    private static string AuthJson() => """
        {
          "token": "access-token",
          "refreshToken": "refresh-token",
          "expiresAt": "2026-01-01T12:00:00Z",
          "email": "ada@example.com",
          "firstName": "Ada",
          "lastName": "Lovelace"
        }
        """;

    [Fact]
    public async Task LoginAsync_PostsToTheLoginRoute_AndMapsTheResponse()
    {
        Respond(HttpStatusCode.OK, AuthJson());

        var service = CreateService();
        var auth = await service.LoginAsync(new LoginRequest
        {
            Email = "ada@example.com",
            Password = "Passw0rd!"
        });

        Assert.NotNull(auth);
        Assert.Equal("access-token", auth.Token);
        Assert.Equal("refresh-token", auth.RefreshToken);
        Assert.Equal("ada@example.com", auth.Email);
        Assert.Equal(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), auth.ExpiresAt);

        var request = Assert.Single(_requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/api/v1/Auth/login", request.RequestUri!.AbsolutePath);

        var body = await request.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("ada@example.com", doc.RootElement.GetProperty("email").GetString());
        Assert.Equal("Passw0rd!", doc.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenTheApiRejectsTheCredentials()
    {
        Respond(HttpStatusCode.Unauthorized, """{"error":"Invalid credentials"}""");

        var service = CreateService();
        var auth = await service.LoginAsync(new LoginRequest { Email = "ada@example.com" });

        Assert.Null(auth);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenTheBodyIsEmpty()
    {
        Respond(HttpStatusCode.OK, string.Empty);

        var service = CreateService();
        var auth = await service.LoginAsync(new LoginRequest { Email = "ada@example.com" });

        Assert.Null(auth);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenTheBodyIsNotJson()
    {
        Respond(HttpStatusCode.OK, "not json at all");

        var service = CreateService();
        var auth = await service.LoginAsync(new LoginRequest { Email = "ada@example.com" });

        Assert.Null(auth);
    }

    [Fact]
    public async Task RegisterAsync_PostsToTheRegisterRoute()
    {
        Respond(HttpStatusCode.OK, AuthJson());

        var service = CreateService();
        var auth = await service.RegisterAsync(new RegisterRequest
        {
            Email = "ada@example.com",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        });

        Assert.NotNull(auth);

        var request = Assert.Single(_requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/api/v1/Auth/register", request.RequestUri!.AbsolutePath);

        var body = await request.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("ada@example.com", doc.RootElement.GetProperty("email").GetString());
        Assert.Equal("Passw0rd!", doc.RootElement.GetProperty("password").GetString());
        Assert.Equal("Passw0rd!", doc.RootElement.GetProperty("confirmPassword").GetString());
    }

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenTheEmailIsTaken()
    {
        Respond(HttpStatusCode.BadRequest, """{"error":"Email already exists"}""");

        var service = CreateService();
        var auth = await service.RegisterAsync(new RegisterRequest { Email = "ada@example.com" });

        Assert.Null(auth);
    }

    [Fact]
    public async Task RefreshAsync_PostsToTheRefreshRoute()
    {
        Respond(HttpStatusCode.OK, AuthJson());

        var service = CreateService();
        var auth = await service.RefreshAsync(new RefreshRequest
        {
            Token = "old-access-token",
            RefreshToken = "old-refresh-token"
        });

        Assert.NotNull(auth);

        var request = Assert.Single(_requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/api/v1/Auth/refresh", request.RequestUri!.AbsolutePath);

        var body = await request.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("old-access-token", doc.RootElement.GetProperty("token").GetString());
        Assert.Equal("old-refresh-token", doc.RootElement.GetProperty("refreshToken").GetString());
    }

    [Fact]
    public async Task RefreshAsync_ReturnsNull_WhenTheRefreshTokenIsRejected()
    {
        Respond(HttpStatusCode.Unauthorized, """{"error":"Invalid or expired refresh token"}""");

        var service = CreateService();
        var auth = await service.RefreshAsync(new RefreshRequest());

        Assert.Null(auth);
    }

    [Fact]
    public async Task LogoutAsync_PostsBothTokensToTheLogoutRoute()
    {
        Respond(HttpStatusCode.OK, """{"message":"Successfully logged out"}""");

        var service = CreateService();
        var revoked = await service.LogoutAsync("access-token", "refresh-token");

        Assert.True(revoked);

        var request = Assert.Single(_requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/api/v1/Auth/logout", request.RequestUri!.AbsolutePath);

        var body = await request.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("access-token", doc.RootElement.GetProperty("token").GetString());
        Assert.Equal("refresh-token", doc.RootElement.GetProperty("refreshToken").GetString());
    }

    [Fact]
    public async Task LogoutAsync_ReturnsFalse_WhenThereIsNoRefreshToken()
    {
        var service = CreateService();
        var revoked = await service.LogoutAsync("access-token", "   ");

        Assert.False(revoked);
        Assert.Empty(_requests);
    }

    [Fact]
    public async Task LogoutAsync_ReturnsFalse_WhenTheApiRefuses()
    {
        Respond(HttpStatusCode.BadRequest, """{"error":"Failed to revoke token"}""");

        var service = CreateService();
        var revoked = await service.LogoutAsync("access-token", "refresh-token");

        Assert.False(revoked);
    }
}
