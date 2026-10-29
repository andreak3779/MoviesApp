using Microsoft.AspNetCore.Http;
using MoviesWeb.Services;

namespace MoviesWeb.Tests.Services;

/// <summary>
/// Verifies that the bearer token is attached to API calls - but not to the
/// auth endpoints, where a stale token would break the sign-in flow.
/// </summary>
public class ApiTokenHandlerTests
{
    private const string BaseUrl = "http://api.test/api/v1/";

    private readonly Mock<IMovieSession> _movieSession = new();

    [Fact]
    public async Task SendAsync_AttachesTheBearerToken()
    {
        _movieSession.Setup(s => s.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("access-token");

        var (handler, inner) = CreateHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };

        await client.GetAsync("Movie");

        Assert.Equal("Bearer", inner.Request!.Headers.Authorization?.Scheme);
        Assert.Equal("access-token", inner.Request.Headers.Authorization?.Parameter);
    }

    [Theory]
    [InlineData("Auth/login")]
    [InlineData("Auth/register")]
    [InlineData("Auth/refresh")]
    public async Task SendAsync_DoesNotAttachATokenToTheAuthEndpoints(string path)
    {
        _movieSession.Setup(s => s.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("access-token");

        var (handler, inner) = CreateHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };

        await client.PostAsync(path, new StringContent("{}"));

        Assert.Null(inner.Request!.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_AttachesNoHeader_WhenThereIsNoToken()
    {
        _movieSession.Setup(s => s.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var (handler, inner) = CreateHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };

        await client.GetAsync("Movie");

        Assert.Null(inner.Request!.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_AttachesNoHeader_WhenThereIsNoSession()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var (handler, inner) = CreateHandler(accessor.Object);
        using var client = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };

        await client.GetAsync("Movie");

        Assert.Null(inner.Request!.Headers.Authorization);
    }

    private (ApiTokenHandler Handler, CapturingHandler Inner) CreateHandler(
        IHttpContextAccessor? accessor = null)
    {
        accessor ??= CreateAccessor();
        var inner = new CapturingHandler();
        var handler = new ApiTokenHandler(accessor, new ServiceProviderStub(_movieSession))
        {
            InnerHandler = inner
        };

        return (handler, inner);
    }

    /// <summary>
    /// Builds an accessor over a request that has a session, which is what
    /// the handler needs before it looks the token up.
    /// </summary>
    private static IHttpContextAccessor CreateAccessor()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext)
            .Returns(new DefaultHttpContext { Session = new FakeSession() });

        return accessor.Object;
    }

    /// <summary>
    /// Resolves the session without pulling in the whole DI container.
    /// </summary>
    private sealed class ServiceProviderStub : IServiceProvider
    {
        private readonly Mock<IMovieSession> _movieSession;

        public ServiceProviderStub(Mock<IMovieSession> movieSession)
        {
            _movieSession = movieSession;
        }

        public object? GetService(Type serviceType) =>
            serviceType == typeof(IMovieSession) ? _movieSession.Object : null;
    }

    /// <summary>
    /// Records the request that reached the network.
    /// </summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
