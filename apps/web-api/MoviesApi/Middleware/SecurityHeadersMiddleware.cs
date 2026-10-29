namespace MoviesApi.Middleware;

public class SecurityHeadersMiddleware
{
    /// <summary>
    /// Paths served by the Swagger UI. The UI injects inline styles and
    /// <c>data:</c> images, which the strict policy below blocks.
    /// </summary>
    private static readonly string[] SwaggerPaths = { "/swagger" };

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            // Prevent MIME type sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // Prevent clickjacking
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            
            // XSS protection (legacy but still useful)
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            // Referrer policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Permissions policy (formerly Feature-Policy)
            context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
            
            // Content Security Policy. The Swagger UI sets inline styles and
            // loads its icons from data: URIs, so it needs a slightly looser
            // policy than the rest of the API. Every other path keeps the
            // strict default.
            context.Response.Headers.Append(
                "Content-Security-Policy",
                IsSwaggerPath(context.Request.Path)
                    ? "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
                      "img-src 'self' data:; object-src 'none'; frame-ancestors 'none';"
                    : "default-src 'self'; object-src 'none'; frame-ancestors 'none';");

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static bool IsSwaggerPath(PathString path) =>
        SwaggerPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
}
