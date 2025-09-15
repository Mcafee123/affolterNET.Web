using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Services;

namespace affolterNET.Web.Bff.Services;

public class BffSessionService(
    IClaimsEnrichmentService claimsEnrichmentService,
    ILogger<BffSessionService> logger,
    IServiceProvider serviceProvider)
    : IBffSessionService
{
    public Task<bool> IsUserAuthenticatedAsync(HttpContext context)
    {
        return Task.FromResult(context.User.Identity?.IsAuthenticated == true);
    }

    public async Task<UserContext?> GetUserContextAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!context.User.Identity?.IsAuthenticated == true)
            return null;

        var accessToken = await GetAccessTokenAsync(context, cancellationToken);
        return await claimsEnrichmentService.EnrichUserContextAsync(context.User, accessToken, cancellationToken);
    }

    public async Task<string?> GetAccessTokenAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!context.User.Identity?.IsAuthenticated == true)
            return null;

        try
        {
            return await context.GetTokenAsync("access_token");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to retrieve access token");
            return null;
        }
    }

    public async Task RefreshTokensAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // TODO: Implement token refresh logic
        // This would typically involve:
        // 1. Get refresh token from authentication properties
        // 2. Call token endpoint with refresh_token grant
        // 3. Update authentication properties with new tokens
        await Task.CompletedTask;
    }

    public async Task RevokeTokensAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!context.User.Identity?.IsAuthenticated == true)
            return;

        try
        {
            var accessToken = await GetAccessTokenAsync(context, cancellationToken);
            if (!string.IsNullOrEmpty(accessToken))
            {
                // TODO: Implement token revocation
                // Call the revocation endpoint if supported by the provider
            }
            
            // Clear custom token cache if service is available
            var tokenCacheCleanup = serviceProvider.GetService<ITokenCacheCleanupService>();
            tokenCacheCleanup?.ClearUserTokens();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to revoke tokens");
        }
    }
}