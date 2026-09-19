using System.Text.Json;
using MoviesApi.Services;

namespace MoviesApi.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        // Only log authenticated user actions
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && ShouldLog(context.Request.Method))
        {
            var action = MapAction(context.Request.Method);
            var entityType = ExtractEntityType(context.Request.Path);
            var entityId = ExtractEntityId(context.Request.Path);
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
            var endpoint = context.Request.Path.ToString();

            // Try to capture request body for CREATE/UPDATE. We do not capture
            // bodies for auth surface routes at all, and we redact known
            // sensitive field names (password, token, refreshToken, ...) from
            // everything we do capture. See SensitiveFieldRedactor.
            object? requestBody = null;
            var dropBody = SensitiveFieldRedactor.ShouldDropBody(endpoint);
            if (!dropBody && action is "CREATE" or "UPDATE")
            {
                try
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(body))
                    {
                        var parsed = JsonSerializer.Deserialize<object>(body);
                        requestBody = SensitiveFieldRedactor.Redact(parsed);
                    }
                    context.Request.Body.Position = 0;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to read request body for audit logging");
                }
            }

            // Log after the request completes
            await _next(context);

            // Log the audit entry. NewValues still carries the redacted body
            // so investigators can see *what* the caller asked for without
            // it containing credentials. Old values would require more
            // complex tracking and are left null for now.
            await auditLogService.LogAsync(
                userId,
                action,
                entityType,
                entityId,
                action == "UPDATE" ? requestBody : null,
                action is "CREATE" or "UPDATE" ? requestBody : null,
                ipAddress,
                userAgent,
                endpoint
            );
        }
        else
        {
            await _next(context);
        }
    }

    private static bool ShouldLog(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "POST" or "PUT" or "DELETE" or "PATCH" => true,
            _ => false
        };
    }

    private static string MapAction(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "POST" => "CREATE",
            "PUT" or "PATCH" => "UPDATE",
            "DELETE" => "DELETE",
            _ => "OTHER"
        };
    }

    private static string ExtractEntityType(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        
        // Look for controller name in path (e.g., /api/v1/movies/123)
        var controllerSegment = segments.FirstOrDefault(s => 
            !s.Equals("api", StringComparison.OrdinalIgnoreCase) && 
            !s.StartsWith("v", StringComparison.OrdinalIgnoreCase));
        
        return controllerSegment ?? "Unknown";
    }

    private static string? ExtractEntityId(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        
        // Last segment is typically the ID
        var idSegment = segments.LastOrDefault();
        
        // Validate it looks like an ID (number or GUID)
        if (int.TryParse(idSegment, out _) || Guid.TryParse(idSegment, out _))
        {
            return idSegment;
        }
        
        return null;
    }
}
