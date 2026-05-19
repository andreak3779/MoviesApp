using System.Collections.Concurrent;

namespace MoviesApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var endpoint = context.Request.Path;
        var isWriteOperation = IsWriteOperation(context.Request.Method);

        var limit = isWriteOperation ? _options.WriteLimit : _options.GlobalLimit;
        var window = TimeSpan.FromSeconds(_options.WindowInSeconds);

        var entry = _entries.GetOrAdd(clientId, _ => new RateLimitEntry());

        bool isAllowed;
        int remaining;

        lock (entry)
        {
            var now = DateTime.UtcNow;
            var windowStart = now - window;

            // Remove old entries
            entry.Requests = entry.Requests.Where(r => r > windowStart).ToList();

            isAllowed = entry.Requests.Count < limit;
            remaining = Math.Max(0, limit - entry.Requests.Count);

            if (isAllowed)
            {
                entry.Requests.Add(now);
                remaining--;
            }
        }

        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
            context.Response.StatusCode = 429;
            context.Response.Headers.Append("Retry-After", _options.WindowInSeconds.ToString());
            context.Response.Headers.Append("X-RateLimit-Limit", limit.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", "0");
            
            var response = new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = _options.WindowInSeconds
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // Add rate limit headers to response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append("X-RateLimit-Limit", limit.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", Math.Max(0, limit - entry.Requests.Count).ToString());
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string GetClientId(HttpContext context)
    {
        // Use user ID if authenticated, otherwise use IP
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }

    private static bool IsWriteOperation(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "POST" or "PUT" or "DELETE" or "PATCH" => true,
            _ => false
        };
    }

    private class RateLimitEntry
    {
        public List<DateTime> Requests { get; set; } = new();
    }
}

public class RateLimitOptions
{
    public int GlobalLimit { get; set; } = 100; // requests per window
    public int WriteLimit { get; set; } = 10; // requests per window for write operations
    public int WindowInSeconds { get; set; } = 60; // window size in seconds
}
