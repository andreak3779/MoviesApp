using System.Net;
using System.Text.Json;
using MoviesApi.Tests.Infrastructure;
using Xunit;

namespace MoviesApi.Tests.Integration;

/// <summary>
/// The Swagger UI is the API's published contract. These tests boot the real
/// API and assert that the document is served, describes the real routes and
/// carries the bearer scheme the Swagger UI needs to authorize a request.
/// </summary>
public class SwaggerEndpointTests : IClassFixture<MoviesApiFactory>
{
    private readonly MoviesApiFactory _factory;

    public SwaggerEndpointTests(MoviesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_IsServed()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SwaggerJson_DescribesTheMovieRoutes()
    {
        var client = _factory.CreateClient();

        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        using var document = JsonDocument.Parse(json);

        var paths = document.RootElement.GetProperty("paths");
        var pathNames = paths.EnumerateObject().Select(p => p.Name).ToList();

        Assert.Contains("/api/v1/Movie", pathNames);
        Assert.Contains("/api/v1/Movie/{id}", pathNames);
        Assert.Contains("/api/v1/Auth/login", pathNames);
        Assert.Contains("/api/v1/Auth/register", pathNames);
        Assert.Contains("/api/v1/FilmGenres", pathNames);
    }

    [Fact]
    public async Task SwaggerJson_DeclaresTheBearerSecurityScheme()
    {
        var client = _factory.CreateClient();

        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        using var document = JsonDocument.Parse(json);

        var components = document.RootElement.GetProperty("components");
        var securitySchemes = components.GetProperty("securitySchemes");

        Assert.True(securitySchemes.TryGetProperty("Bearer", out var bearer));
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
    }

    [Fact]
    public async Task SwaggerUi_IsServed()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SwaggerUi_RootRedirectsToTheUi()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SwaggerResponses_CarryTheContentSecurityPolicyTheUiNeeds()
    {
        // The strict API policy blocks the inline styles and data: images the
        // Swagger UI uses, so the UI would render unstyled without the
        // relaxed policy on the /swagger paths.
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var values));
        var policy = string.Join("; ", values);
        Assert.Contains("style-src 'self' 'unsafe-inline'", policy);
        Assert.Contains("img-src 'self' data:", policy);
    }
}
