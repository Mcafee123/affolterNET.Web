using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace affolterNET.Web.Bff.Middleware;

/// <summary>
/// Middleware that prevents unauthorized (401) and forbidden (403) responses from being redirected to login pages for API routes.
/// Instead, it returns proper JSON error responses for AJAX/API calls.
/// </summary>
public class NoUnauthorizedRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NoUnauthorizedRedirectMiddleware> _logger;
    private readonly string[] _apiRoutePrefixes;

    public NoUnauthorizedRedirectMiddleware(
        RequestDelegate next, 
        ILogger<NoUnauthorizedRedirectMiddleware> logger,
        params string[] apiRoutePrefixes)
    {
        _next = next;
        _logger = logger;
        _apiRoutePrefixes = apiRoutePrefixes;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Check if this is an API route and we have an unauthorized/forbidden response
        if (IsApiRoute(context.Request.Path) && 
            (context.Response.StatusCode == 401 || context.Response.StatusCode == 403))
        {
            // Ensure we return JSON error instead of redirect
            if (!context.Response.HasStarted)
            {
                _logger.LogDebug("Intercepting {StatusCode} response for API route: {Path}", 
                    context.Response.StatusCode, context.Request.Path);

                context.Response.ContentType = "application/json";
                
                var errorResponse = new
                {
                    error = context.Response.StatusCode == 401 ? "Unauthorized" : "Forbidden",
                    message = context.Response.StatusCode == 401 ? "Authentication required" : "Access denied",
                    statusCode = context.Response.StatusCode,
                    path = context.Request.Path.Value,
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