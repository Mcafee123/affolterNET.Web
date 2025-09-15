using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace affolterNET.Web.Bff.Middleware;

/// <summary>
/// Middleware that handles 404 responses for API routes, returning JSON instead of HTML error pages.
/// This ensures that API calls receive appropriate JSON error responses rather than HTML 404 pages.
/// </summary>
public class ApiNotFoundMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiNotFoundMiddleware> _logger;
    private readonly string[] _apiRoutePrefixes;

    public ApiNotFoundMiddleware(
        RequestDelegate next, 
        ILogger<ApiNotFoundMiddleware> logger,
        params string[] apiRoutePrefixes)
    {
        _next = next;
        _logger = logger;
        _apiRoutePrefixes = apiRoutePrefixes;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Check if this is a 404 response for an API route
        if (context.Response.StatusCode == 404 && IsApiRoute(context.Request.Path))
        {
            if (!context.Response.HasStarted)
            {
                _logger.LogDebug("Intercepting 404 response for API route: {Path}", context.Request.Path);

                context.Response.ContentType = "application/json";
                
                var errorResponse = new
                {
                    error = "Not Found",
                    message = "The requested API endpoint was not found",
                    statusCode = 404,
                    path = context.Request.Path.Value,
                    method = context.Request.Method,
                    timestamp = DateTimeOffset.UtcNow
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }

    private bool IsApiRoute(PathString path)
    {
        return _apiRoutePrefixes.Any(prefix => path.StartsWithSegments(prefix));
    }
}