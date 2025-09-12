using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using affolterNET.Auth.Core.Models;
using affolterNET.Auth.Core.Services;
using affolterNET.Auth.Bff.Configuration;

namespace affolterNET.Auth.Bff.Services;

public class BffSessionService : IBffSessionService
{
    private readonly IClaimsEnrichmentService _claimsEnrichmentService;
    private readonly ILogger<BffSessionService> _logger;
    private readonly BffAuthOptions _options;

    public BffSessionService(
        IClaimsEnrichmentService claimsEnrichmentService,
        ILogger<BffSessionService> logger,
        IOptions<BffAuthOptions> options)
    {
        _claimsEnrichmentService = claimsEnrichmentService;
        _logger = logger;
        _options = options.Value;
    }

    public Task<bool> IsUserAuthenticatedAsync(HttpContext context)
    {
        return Task.FromResult(context.User.Identity?.IsAuthenticated == true);
    }

    public async Task<UserContext?> GetUserContextAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!context.User.Identity?.IsAuthenticated == true)
            return null;

        var accessToken = await GetAccessTokenAsync(context, cancellationToken);
        return await _claimsEnrichmentService.EnrichUserContextAsync(context.User, accessToken, cancellationToken);
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
            _logger.LogWarning(ex, "Failed to retrieve access token");
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
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke tokens");
        }
    }
}