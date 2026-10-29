using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using MoviesApi.Middleware;
using Xunit;

namespace MoviesApi.Tests.Middleware;

/// <summary>
/// The exception handling middleware is the API's last line of defence: it
/// turns any uncaught exception into an RFC 7807 response. What matters is
/// that it never leaks the stack trace, that it maps the exception types the
/// application throws onto the right status code, and that it stays quiet
/// when the response has already started - writing a body then would throw
/// a second exception and take the connection down.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task UnhandledException_BecomesAProblemDetailsResponse()
    {
        var context = await RunAsync(new InvalidOperationException("boom"));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        var problem = await ReadProblemAsync(context);
        Assert.Equal(500, problem.GetProperty("status").GetInt32());
        Assert.Equal("Internal server error", problem.GetProperty("title").GetString());
        Assert.Equal("https://httpstatuses.com/500", problem.GetProperty("type").GetString());
        Assert.True(problem.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task InternalServerError_DoesNotLeakTheExceptionMessage()
    {
        var context = await RunAsync(new InvalidOperationException("connection string is secret"));

        var body = await ReadBodyAsync(context);
        Assert.DoesNotContain("secret", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnauthorizedAccessException_BecomesA401()
    {
        var context = await RunAsync(new UnauthorizedAccessException("User ID not found"));

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);

        var problem = await ReadProblemAsync(context);
        Assert.Equal(401, problem.GetProperty("status").GetInt32());
        Assert.Equal("Unauthorized", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task ArgumentException_BecomesA400()
    {
        var context = await RunAsync(new ArgumentException("pageSize must be positive"));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var problem = await ReadProblemAsync(context);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Equal("Bad request", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task KeyNotFoundException_BecomesA404()
    {
        var context = await RunAsync(new KeyNotFoundException("No such movie"));

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);

        var problem = await ReadProblemAsync(context);
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("Not found", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task ClientErrors_KeepTheirMessage()
    {
        // A 4xx is caused by the caller's request, so saying what was wrong
        // helps them fix it. Only 5xx hides the detail.
        var context = await RunAsync(new ArgumentException("pageSize must be positive"));

        var problem = await ReadProblemAsync(context);
        Assert.Equal("pageSize must be positive", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ResponseThatAlreadyStarted_IsLeftAlone()
    {
        // Writing a second body would throw, so the middleware returns
        // without touching the response.
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("boom"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Empty(await ReadBodyAsync(context));
    }

    [Fact]
    public async Task SuccessfulRequest_PassesThroughUntouched()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            async ctx => ctx.Response.StatusCode = StatusCodes.Status204NoContent,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.Empty(await ReadBodyAsync(context));
    }

    // ---- helpers -------------------------------------------------------

    /// <summary>
    /// Runs the middleware over a context whose downstream always throws.
    /// </summary>
    private static async Task<DefaultHttpContext> RunAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        return context;
    }

    private static async Task<string> ReadBodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static async Task<JsonElement> ReadProblemAsync(DefaultHttpContext context)
    {
        var body = await ReadBodyAsync(context);
        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// A response feature that reports the response as already started, which
    /// is what happens when the application has written part of a response
    /// before an exception is thrown.
    /// </summary>
    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public string? ReasonPhrase { get; set; }

        public bool HasStarted => true;

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }
    }
}
