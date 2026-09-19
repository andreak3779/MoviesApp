using MoviesApi.Services;
using Xunit;

namespace MoviesApi.Tests;

/// <summary>
/// Covers the audit-log redaction rules from
/// docs/SECURITY-INCIDENT-2026-09-14.md.
/// </summary>
public class SensitiveFieldRedactorTests
{
    [Theory]
    [InlineData("password")]
    [InlineData("Password")]
    [InlineData("PASSWORD")]
    [InlineData("currentPassword")]
    [InlineData("newPassword")]
    [InlineData("confirmPassword")]
    [InlineData("token")]
    [InlineData("accessToken")]
    [InlineData("refreshToken")]
    [InlineData("secret")]
    [InlineData("secretKey")]
    [InlineData("apiKey")]
    public void Redact_ReplacesAllKnownSensitiveFields(string field)
    {
        var input = new Dictionary<string, object?>
        {
            [field] = "hunter2",
            ["title"] = "Inception",
        };

        var redacted = (IDictionary<string, object?>)SensitiveFieldRedactor.Redact(input)!;

        Assert.Equal("[redacted]", redacted[field]);
        Assert.Equal("Inception", redacted["title"]);
    }

    [Fact]
    public void Redact_WalksNestedObjects()
    {
        var input = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["email"] = "alice@example.com",
                ["password"] = "hunter2",
            },
        };

        var redacted = (IDictionary<string, object?>)SensitiveFieldRedactor.Redact(input)!;
        var nested = (IDictionary<string, object?>)redacted["user"]!;

        Assert.Equal("alice@example.com", nested["email"]);
        Assert.Equal("[redacted]", nested["password"]);
    }

    [Fact]
    public void Redact_WalksArrays()
    {
        var input = new Dictionary<string, object?>
        {
            ["items"] = new List<object?>
            {
                new Dictionary<string, object?> { ["token"] = "secret", ["value"] = 1 },
                new Dictionary<string, object?> { ["token"] = "also-secret", ["value"] = 2 },
            },
        };

        var redacted = (IDictionary<string, object?>)SensitiveFieldRedactor.Redact(input)!;
        var items = (List<object?>)redacted["items"]!;

        foreach (var item in items.Cast<IDictionary<string, object?>>())
        {
            Assert.Equal("[redacted]", item["token"]);
        }
    }

    [Fact]
    public void Redact_ReturnsNullForNull()
    {
        Assert.Null(SensitiveFieldRedactor.Redact(null));
    }

    [Theory]
    [InlineData("/api/v1/Auth/login")]
    [InlineData("/api/v1/Auth/register")]
    [InlineData("/api/v1/Auth/refresh")]
    [InlineData("/api/v1/Auth/logout")]
    [InlineData("/api/v1/Auth/login/extra")]
    public void ShouldDropBody_TrueForAuthRoutes(string path)
    {
        Assert.True(SensitiveFieldRedactor.ShouldDropBody(path));
    }

    [Theory]
    [InlineData("/api/v1/Movies")]
    [InlineData("/api/v1/FilmGenres")]
    [InlineData("/api/v1/MediaTypes/2")]
    public void ShouldDropBody_FalseForNonAuthRoutes(string path)
    {
        Assert.False(SensitiveFieldRedactor.ShouldDropBody(path));
    }
}
