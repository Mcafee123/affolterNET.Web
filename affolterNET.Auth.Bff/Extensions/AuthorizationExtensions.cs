using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace affolterNET.Auth.Bff.Extensions;

/// <summary>
/// Extension methods for authorization middleware
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds middleware that returns 401 for API routes instead of redirecting to login
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="apiRoutePrefix">The API route prefix (e.g., "/api")</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseNoUnauthorizedRedirect(this IApplicationBuilder app, string apiRoutePrefix = "/api")
    {
        return app.Use(async (context, next) =>
        {
            await next();

            // If response is 401 or 403 and it's an API route
            if ((context.Response.StatusCode == 401 || context.Response.StatusCode == 403) &&
                context.Request.Path.StartsWithSegments(apiRoutePrefix))
            {
                // Ensure we return JSON error instead of redirect
                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync($$"""
                    {
                        "error": "{{(context.Response.StatusCode == 401 ? "Unauthorized" : "Forbidden")}}",
                        "message": "{{(context.Response.StatusCode == 401 ? "Authentication required" : "Access denied")}}",
                        "statusCode": {{context.Response.StatusCode}}
                    }
                    """);
                }
            }
        });
    }
}

/// <summary>
/// Extension methods for handling 404 responses
/// </summary>
public static class NotFoundExtensions
{
    /// <summary>
    /// Maps a 404 handler for API routes that returns JSON instead of HTML
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="pattern">The route pattern (e.g., "/api/{**segment}")</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder MapNotFound(this IApplicationBuilder app, string pattern)
    {
        return app.Use(async (context, next) =>
        {
            await next();

            // If we get a 404 and it matches the API pattern
            if (context.Response.StatusCode == 404 && 
                context.Request.Path.StartsWithSegments("/api"))
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync($$"""
                    {
                        "error": "Not Found",
                        "message": "The requested API endpoint was not found",
                        "statusCode": 404,
                        "path": "{{context.Request.Path}}"
                    }
                    """);
                }
            }
        });
    }
}