using System.Net;
using System.Text.Json;

namespace MoviesApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = GetErrorMessage(exception),
            details = "An error occurred while processing your request",
            path = context.Request.Path
        };

        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static string GetErrorMessage(Exception ex) => ex switch
    {
        UnauthorizedAccessException => "Unauthorized",
        ArgumentException => "Invalid request",
        KeyNotFoundException => "Resource not found",
        _ => "Internal server error"
    };
}
