using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MoviesApi.Middleware;
using Xunit;

namespace MoviesApi.Tests.Middleware;

/// <summary>
/// Covers the security headers the API returns, in particular the content
/// security policy. The Swagger UI sets inline styles and loads its icons
/// from data: URIs, so it needs a looser policy than the rest of the API -
/// but only on the Swagger paths.
/// </summary>
public class SecurityHeadersMiddlewareTests : IAsyncLifetime
{
    private IHost _host = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IHostEnvironment>(
                            new TestHostEnvironment { EnvironmentName = Environments.Production });
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<SecurityHeadersMiddleware>();
                        app.Run(async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsync("ok");
                        });
                    });
            })
            .StartAsync();

        _client = _host.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }

    private async Task<string> GetCspAsync(string path)
    {
        var response = await _client.GetAsync(path);
        return response.Headers.TryGetValues("Content-Security-Policy", out var values)
            ? string.Join("; ", values)
            : string.Empty;
    }

    [Fact]
    public async Task NonSwaggerPath_UsesStrictContentSecurityPolicy()
    {
        var csp = await GetCspAsync("/api/v1/Movie");

        Assert.Equal("default-src 'self'; object-src 'none'; frame-ancestors 'none';", csp);
    }

    [Theory]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    [InlineData("/swagger/v1/swagger.json")]
    public async Task SwaggerPath_AllowsInlineStylesAndDataImages(string path)
    {
        var csp = await GetCspAsync(path);

        Assert.Contains("style-src 'self' 'unsafe-inline'", csp);
        Assert.Contains("img-src 'self' data:", csp);
        Assert.Contains("script-src 'self'", csp);
        Assert.Contains("object-src 'none'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
    }

    [Fact]
    public async Task SwaggerPath_DoesNotAllowInlineScript()
    {
        var csp = await GetCspAsync("/swagger/index.html");

        Assert.DoesNotContain("script-src 'self' 'unsafe-inline'", csp);
    }

    [Fact]
    public async Task NonSwaggerPath_DoesNotAllowInlineStyles()
    {
        var csp = await GetCspAsync("/api/v1/Movie");

        Assert.DoesNotContain("unsafe-inline", csp);
    }

    [Fact]
    public async Task SwaggerPrefixMatch_DoesNotLeakToOtherPaths()
    {
        // A path that merely starts with the same characters is not a Swagger
        // path and must keep the strict policy.
        var csp = await GetCspAsync("/swaggerger");

        Assert.DoesNotContain("unsafe-inline", csp);
    }

    [Fact]
    public async Task AlwaysSetsTheStandardSecurityHeaders()
    {
        var response = await _client.GetAsync("/api/v1/Movie");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("camera=(), microphone=(), geolocation=()",
            response.Headers.GetValues("Permissions-Policy").Single());
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "MoviesApi.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
