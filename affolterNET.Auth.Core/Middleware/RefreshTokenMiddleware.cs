using affolterNET.Auth.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace affolterNET.Auth.Core.Middleware;

/// <summary>
/// Middleware that automatically refreshes authentication tokens when they are expired or about to expire
/// </summary>
public class RefreshTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenRefreshService _tokenRefreshService;
    private readonly ILogger<RefreshTokenMiddleware> _logger;
    private readonly string _oidcScheme;

    public RefreshTokenMiddleware(
        RequestDelegate next,
        TokenRefreshService tokenRefreshService,
        ILogger<RefreshTokenMiddleware> logger,
        string oidcScheme = "OpenIdConnect")
    {
        _next = next;
        _tokenRefreshService = tokenRefreshService;
        _logger = logger;
        _oidcScheme = oidcScheme;
    }

    /// <summary>
    /// Processes the HTTP request and refreshes tokens if necessary
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // If the user is not authenticated or no access token is present, skip to next middleware
        var tokenExpiresAt = await _tokenRefreshService.ExpiresAt();
        if (context.User.Identity?.IsAuthenticated != true || tokenExpiresAt == null)
        {
            await _next(context);
            return;
        }

        if (await _tokenRefreshService.IsExpired())
        {
            _logger.LogDebug("Access token is expired, attempting to refresh");
            var result = await _tokenRefreshService.RefreshTokensAsync();
            if (result)
            {
                _logger.LogDebug("Tokens have been refreshed successfully");
                await _next(context);
                return;
            }

            // Refresh failed (e.g., expired refresh token). Sign the user out to force re-authentication.
            _logger.LogWarning("Token refresh failed, signing user out to force re-authentication");
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(_oidcScheme);
            // After sign-out, short-circuit the pipeline to avoid using an invalid principal
            return;
        }
 
        // Continue to the next middleware in the pipeline
        await _next(context);
    }
}