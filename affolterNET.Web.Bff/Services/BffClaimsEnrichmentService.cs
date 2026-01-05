using System.Security.Claims;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Extensions;
using Microsoft.Extensions.Logging;
using affolterNET.Web.Core.Services;
using affolterNET.Web.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace affolterNET.Web.Bff.Services;

public class BffClaimsEnrichmentService(
    IPermissionService permissionService,
    IOptions<OidcClaimTypeOptions> oidcClaimTypeOptions,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BffClaimsEnrichmentService> logger)
    : IClaimsEnrichmentService
{

    private readonly OidcClaimTypeOptions _claimTypes = oidcClaimTypeOptions.Value;

    public async Task<UserContext> EnrichUserContextAsync(ClaimsPrincipal principal, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirstClaimValue(_claimTypes.Subject);
        var username = principal.FindFirstClaimValue(_claimTypes.PreferredUsername);
        var email = principal.FindFirstClaimValue(_claimTypes.Email);
        var name = principal.FindFirstClaimValue(_claimTypes.Name);

        // Extract roles from both standard role claims and Keycloak 'roles' claims
        // ToDo: is "ClaimTypes.Role" necessary?
        var roles = principal.FindAll(_claimTypes.Roles)
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        // Get access token from authentication properties if not provided
        if (string.IsNullOrEmpty(accessToken))
        {
            accessToken = principal.FindFirst("access_token")?.Value;

            // Fall back to HttpContext authentication properties
            if (string.IsNullOrEmpty(accessToken) && httpContextAccessor.HttpContext != null)
            {
                try
                {
                    accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to get access token from authentication properties");
                }
            }
        }

        var permissions = Array.Empty<Permission>();
        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(userId))
        {
            try
            {
                permissions = (await permissionService.GetUserPermissionsAsync(userId, accessToken, cancellationToken)).ToArray();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load permissions for user {UserId}", userId);
            }
        }

        return new UserContext
        {
            UserId = userId,
            Username = username,
            Email = email,
            Name = name,
            Roles = roles,
            Permissions = permissions,
            Claims = principal.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count() == 1
                        ? (object)g.First().Value
                        : g.Select(c => c.Value).ToArray()
                )
        };
    }

    public async Task<ClaimsPrincipal> EnrichClaimsAsync(ClaimsPrincipal principal, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        var userContext = await EnrichUserContextAsync(principal, accessToken, cancellationToken);

        var identity = new ClaimsIdentity(principal.Identity);

        // Add permission claims
        foreach (var permission in userContext.Permissions)
        {
            identity.AddClaim(new Claim("permission", $"{permission.Resource}:{permission.Action}"));
        }

        return new ClaimsPrincipal(identity);
    }

    private string FindFirstClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value ?? string.Empty;
    }
}