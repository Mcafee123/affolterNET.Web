using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using affolterNET.Auth.Core.Services;
using affolterNET.Auth.Core.Models;
using affolterNET.Auth.Bff.Configuration;

namespace affolterNET.Auth.Bff.Services;

public class BffClaimsEnrichmentService : IClaimsEnrichmentService
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<BffClaimsEnrichmentService> _logger;
    private readonly BffAuthOptions _options;

    public BffClaimsEnrichmentService(
        IPermissionService permissionService,
        ILogger<BffClaimsEnrichmentService> logger,
        IOptions<BffAuthOptions> options)
    {
        _permissionService = permissionService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<UserContext> EnrichUserContextAsync(ClaimsPrincipal principal, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var username = principal.FindFirst("preferred_username")?.Value ?? string.Empty;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        // Get access token from authentication properties if not provided
        accessToken ??= principal.FindFirst("access_token")?.Value;
        
        var permissions = Array.Empty<Permission>();
        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(userId))
        {
            try
            {
                permissions = (await _permissionService.GetUserPermissionsAsync(userId, accessToken, cancellationToken)).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load permissions for user {UserId}", userId);
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
            Claims = principal.Claims.ToDictionary(c => c.Type, c => (object)c.Value)
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