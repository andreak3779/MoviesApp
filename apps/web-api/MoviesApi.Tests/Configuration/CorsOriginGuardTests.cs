using MoviesApi.Configuration;
using Xunit;

namespace MoviesApi.Tests.Configuration;

/// <summary>
/// Covers the Production CORS guard. The previous implementation matched
/// placeholders by prefix, so the legitimate development origins
/// (<c>http://localhost:4200</c> and friends) were rejected and the container
/// stack could not start.
/// </summary>
public class CorsOriginGuardTests
{
    [Theory]
    [InlineData("http://localhost:4200")]
    [InlineData("http://localhost:4000")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://moviesapp.example.com")]
    [InlineData("http://web-api:8080")]
    public void FindPlaceholder_RealOrigin_ReturnsNull(string origin)
    {
        Assert.Null(CorsOriginGuard.FindPlaceholder(origin));
    }

    [Theory]
    [InlineData("https://your-production-domain.com")]
    [InlineData("http://localhost")]
    [InlineData("HTTP://LOCALHOST")]
    [InlineData("http://localhost ")]
    public void FindPlaceholder_PlaceholderOrigin_ReturnsThePlaceholder(string origin)
    {
        Assert.NotNull(CorsOriginGuard.FindPlaceholder(origin));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FindPlaceholder_EmptyOrigin_ReturnsNull(string? origin)
    {
        Assert.Null(CorsOriginGuard.FindPlaceholder(origin));
    }

    [Fact]
    public void EnsureNoPlaceholders_AllRealOrigins_DoesNotThrow()
    {
        var origins = new[]
        {
            "http://localhost:4200",
            "http://localhost:4000",
            "https://moviesapp.example.com",
        };

        CorsOriginGuard.EnsureNoPlaceholders(origins);
    }

    [Fact]
    public void EnsureNoPlaceholders_ContainsPlaceholder_Throws()
    {
        var origins = new[]
        {
            "http://localhost:4200",
            "https://your-production-domain.com",
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => CorsOriginGuard.EnsureNoPlaceholders(origins));

        Assert.Contains("your-production-domain.com", exception.Message);
    }

    [Fact]
    public void EnsureNoPlaceholders_ContainsBareLocalhost_Throws()
    {
        var origins = new[] { "http://localhost" };

        Assert.Throws<InvalidOperationException>(
            () => CorsOriginGuard.EnsureNoPlaceholders(origins));
    }
}
