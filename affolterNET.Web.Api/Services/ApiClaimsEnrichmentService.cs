using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using affolterNET.Web.Core.Services;
using affolterNET.Web.Core.Models;
using affolterNET.Web.Api.Configuration;

namespace affolterNET.Web.Api.Services;

public class ApiClaimsEnrichmentService(
    IPermissionService permissionService,
    ILogger<ApiClaimsEnrichmentService> logger)
    : IClaimsEnrichmentService
{
    public async Task<UserContext> EnrichUserContextAsync(ClaimsPrincipal principal, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var username = principal.FindFirst("preferred_username")?.Value ?? string.Empty;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        // Extract roles from both standard role claims and Keycloak 'roles' claims
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Concat(principal.FindAll("roles").Select(c => c.Value))
            .Distinct()
            .ToList();
        
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
}