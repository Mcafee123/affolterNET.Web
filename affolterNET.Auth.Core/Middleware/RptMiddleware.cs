using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using affolterNET.Auth.Core.Services;

namespace affolterNET.Auth.Core.Middleware;

public class RptMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RptMiddleware> _logger;

    public RptMiddleware(RequestDelegate next, ILogger<RptMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AuthClaimsService authClaimsService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var accessToken = await context.GetTokenAsync("access_token");
                if (accessToken == null)
                {
                    _logger.LogWarning("Access token is missing for authenticated user");
                }
                else
                {
                    await authClaimsService.EnrichUserWithPermissionsAndRoles(accessToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enrich user claims with RPT permissions");
                // Continue without enrichment
            }
        }

        await _next(context);
    }

    // Legacy method for interface-based enrichment (kept for backward compatibility)
    public async Task InvokeAsync(HttpContext context, IClaimsEnrichmentService claimsEnrichmentService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var accessToken = ExtractAccessToken(context);
                var enrichedPrincipal = await claimsEnrichmentService.EnrichClaimsAsync(context.User, accessToken);
                context.User = enrichedPrincipal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enrich user claims");
                // Continue without enrichment
            }
        }

        await _next(context);
    }

    private static string? ExtractAccessToken(HttpContext context)
    {
        // Try to get from Authorization header first
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader["Bearer ".Length..];
        }

        // Try to get from authentication result
        if (context.User.Identity?.IsAuthenticated == true)
        {
            return context.User.FindFirst("access_token")?.Value;
        }

        return null;
    }
}