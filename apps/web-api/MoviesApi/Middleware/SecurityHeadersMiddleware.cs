namespace MoviesApi.Middleware;

public class SecurityHeadersMiddleware
{
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
            
            // Content Security Policy
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; object-src 'none'; frame-ancestors 'none';");

            // HSTS (only in production)
            if (!context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                context.Response.Headers.Append("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
