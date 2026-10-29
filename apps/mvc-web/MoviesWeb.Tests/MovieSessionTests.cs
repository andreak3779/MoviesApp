using Microsoft.AspNetCore.Http;
using MoviesWeb.Models;
using MoviesWeb.Services;

namespace MoviesWeb.Tests.Services;

/// <summary>
/// Verifies that the session keeps the API tokens and refreshes an expiring
/// access token before it is used.
/// </summary>
public class MovieSessionTests
{
    private readonly Mock<IApiAuthService> _authService = new();
    private readonly Mock<ILogger<MovieSession>> _logger = new();
    private readonly FakeSession _session = new();

    /// <summary>Builds a session backed by the fake in-memory store.</summary>
    private MovieSession CreateSession() =>
        CreateSessionFor(new DefaultHttpContext { Session = _session });

    /// <summary>Builds a session for an explicit HTTP context.</summary>
    private MovieSession CreateSessionFor(HttpContext? httpContext)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        return new MovieSession(accessor.Object, _authService.Object, _logger.Object);
    }

    private static AuthResponse Auth(
        string token = "access-token",
        string refreshToken = "refresh-token",
        DateTime? expiresAt = null) => new()
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(15),
            Email = "ada@example.com"
        };

    [Fact]
    public void SignIn_StoresTheTokens()
    {
        var session = CreateSession();

        session.SignIn(Auth());

        Assert.True(session.IsSignedIn);
        Assert.Equal("access-token", session.AccessToken);
        Assert.Equal("refresh-token", session.RefreshToken);
        Assert.Equal("ada@example.com", session.Email);
    }

    [Fact]
    public void SignOut_ClearsTheTokens()
    {
        var session = CreateSession();
        session.SignIn(Auth());

        session.SignOut();

        Assert.False(session.IsSignedIn);
        Assert.Null(session.AccessToken);
        Assert.Null(session.RefreshToken);
        Assert.Null(session.Email);
    }

    [Fact]
    public void IsSignedIn_IsFalse_WithoutASession()
    {
        var session = CreateSessionFor(httpContext: null);

        Assert.False(session.IsSignedIn);
        Assert.Null(session.AccessToken);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsTheStoredToken_WhenItIsStillValid()
    {
        var session = CreateSession();
        session.SignIn(Auth(expiresAt: DateTime.UtcNow.AddMinutes(15)));

        var token = await session.GetAccessTokenAsync();

        Assert.Equal("access-token", token);
        _authService.Verify(
            s => s.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccessTokenAsync_RefreshesAnExpiringToken()
    {
        var session = CreateSession();
        session.SignIn(Auth(expiresAt: DateTime.UtcNow.AddSeconds(10)));

        _authService.Setup(s => s.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Auth("new-access-token", "new-refresh-token"));

        var token = await session.GetAccessTokenAsync();

        Assert.Equal("new-access-token", token);
        Assert.Equal("new-access-token", session.AccessToken);
        Assert.Equal("new-refresh-token", session.RefreshToken);
    }

    [Fact]
    public async Task GetAccessTokenAsync_SignsOut_WhenTheRefreshIsRejected()
    {
        var session = CreateSession();
        session.SignIn(Auth(expiresAt: DateTime.UtcNow.AddSeconds(10)));

        _authService.Setup(s => s.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);

        var token = await session.GetAccessTokenAsync();

        Assert.Null(token);
        Assert.False(session.IsSignedIn);
    }

    [Fact]
    public async Task GetAccessTokenAsync_SignsOut_WhenTheRefreshRequestFails()
    {
        var session = CreateSession();
        session.SignIn(Auth(expiresAt: DateTime.UtcNow.AddSeconds(10)));

        _authService.Setup(s => s.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var token = await session.GetAccessTokenAsync();

        Assert.Null(token);
        Assert.False(session.IsSignedIn);
    }

    [Fact]
    public async Task GetAccessTokenAsync_SignsOut_WhenTheRefreshTokenIsMissing()
    {
        var session = CreateSession();
        session.SignIn(Auth(expiresAt: DateTime.UtcNow.AddSeconds(10)));
        _session.Remove(MovieSession.RefreshTokenKey);

        var token = await session.GetAccessTokenAsync();

        Assert.Null(token);
        Assert.False(session.IsSignedIn);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsNull_WithoutAToken()
    {
        var session = CreateSession();

        Assert.Null(await session.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsNull_WithoutASession()
    {
        var session = CreateSessionFor(httpContext: null);

        Assert.Null(await session.GetAccessTokenAsync());
    }
}
