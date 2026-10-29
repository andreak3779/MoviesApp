using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoviesApi.Middleware;
using Xunit;

namespace MoviesApi.Tests.Middleware;

/// <summary>
/// The rate limiter keys on the caller's id when the request is
/// authenticated and falls back to the IP address when it is not.
///
/// That only works if authentication runs first. The middleware used to be
/// registered before <c>UseAuthentication</c>, which made the per-user branch
/// unreachable and put every caller of an instance behind the same IP
/// bucket. These tests pin the behaviour down.
/// </summary>
public class RateLimitingMiddlewareTests
{
    private const int WriteLimit = 2;

    /// <summary>
    /// The header the test uses to say who the caller is. The API derives
    /// the same thing from the JWT's name-identifier claim.
    /// </summary>
    private const string UserHeader = "X-Test-User";

    /// <summary>
    /// Builds a host whose pipeline authenticates before rate limiting, the
    /// way the API is wired. The caller is taken from a request header so a
    /// single host - and therefore a single rate limiter - can serve more
    /// than one user.
    /// </summary>
    private static async Task<IHost> BuildHostAsync()
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.Configure<RateLimitOptions>(options =>
                        {
                            options.GlobalLimit = 1000;
                            options.WriteLimit = WriteLimit;
                            options.WindowInSeconds = 60;
                        });
                    })
                    .Configure(app =>
                    {
                        // Authentication first: the limiter reads the
                        // principal this middleware establishes.
                        app.Use(async (context, next) =>
                        {
                            if (context.Request.Headers.TryGetValue(UserHeader, out var userId)
                                && !string.IsNullOrWhiteSpace(userId))
                            {
                                context.User = new ClaimsPrincipal(new ClaimsIdentity(
                                    new[]
                                    {
                                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                                    },
                                    "TestAuth"));
                            }

                            await next();
                        });

                        app.UseMiddleware<RateLimitingMiddleware>();

                        app.Run(async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsync("ok");
                        });
                    });
            })
            .StartAsync();

        return host;
    }

    [Fact]
    public async Task Requests_UnderTheLimit_AreAllowed()
    {
        using var host = await BuildHostAsync();
        var client = host.GetTestClient();

        for (var i = 0; i < WriteLimit; i++)
        {
            var response = await PostAsync(client, "user-1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Requests_OverTheLimit_AreRejectedWith429()
    {
        using var host = await BuildHostAsync();
        var client = host.GetTestClient();

        for (var i = 0; i < WriteLimit; i++)
        {
            await PostAsync(client, "user-1");
        }

        var response = await PostAsync(client, "user-1");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.True(response.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task DifferentUsers_GetSeparateBuckets()
    {
        // The per-user branch: two callers from the same address must not
        // share a bucket, otherwise one user's traffic throttles another's.
        using var host = await BuildHostAsync();
        var client = host.GetTestClient();

        for (var i = 0; i < WriteLimit; i++)
        {
            await PostAsync(client, "user-1");
        }

        // The first caller is now over their limit...
        var blocked = await PostAsync(client, "user-1");
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);

        // ...but the second caller is not, because the buckets are keyed on
        // the authenticated id rather than the IP address.
        var allowed = await PostAsync(client, "user-2");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    [Fact]
    public async Task AnonymousRequests_AreKeyedOnTheAddress()
    {
        // Without an authenticated principal there is no id to key on, so
        // every anonymous caller shares one bucket.
        using var host = await BuildHostAsync();
        var client = host.GetTestClient();

        for (var i = 0; i < WriteLimit; i++)
        {
            await client.PostAsync("/api/v1/Movie", new StringContent("{}"));
        }

        var response = await client.PostAsync("/api/v1/Movie", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task Responses_CarryTheRateLimitHeaders()
    {
        using var host = await BuildHostAsync();
        var client = host.GetTestClient();

        var response = await PostAsync(client, "user-1");

        Assert.True(response.Headers.Contains("X-RateLimit-Limit"));
        Assert.True(response.Headers.Contains("X-RateLimit-Remaining"));
    }

    /// <summary>
    /// Issues a write as the given user.
    /// </summary>
    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Movie")
        {
            Content = new StringContent("{}")
        };
        request.Headers.Add(UserHeader, userId);
        return client.SendAsync(request);
    }
}
