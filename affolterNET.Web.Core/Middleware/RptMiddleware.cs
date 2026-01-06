using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Services;

namespace affolterNET.Web.Core.Middleware;

/// <summary>
/// Middleware that enriches authenticated users with RPT-based permission claims.
/// Uses PermissionService as the single source for permission retrieval.
/// </summary>
public class RptMiddleware(RequestDelegate next, ILogger<RptMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        IPermissionService permissionService,
        IOptions<OidcClaimTypeOptions> claimTypeOptions)
    {
        logger.LogDebug("RptMiddleware invoked for path: {Path}", context.Request.Path);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            logger.LogDebug("User is authenticated: {User}", context.User.Identity.Name);
            try
            {
                var claimTypes = claimTypeOptions.Value;
                var userId = context.User.FindFirstValue(claimTypes.Subject);

                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("User ID claim '{ClaimType}' not found for authenticated user", claimTypes.Subject);
                }
                else
                {
                    var accessToken = await context.GetTokenAsync("access_token");
                    if (accessToken == null)
                    {
                        logger.LogWarning("Access token is missing for authenticated user {UserId}", userId);
                    }
                    else
                    {
                        logger.LogDebug("Access token found, fetching permissions for user {UserId}...", userId);
                        var permissions = await permissionService.GetUserPermissionsAsync(userId, accessToken);

                        if (permissions.Count > 0)
                        {
                            var identity = (ClaimsIdentity)context.User.Identity;
                            foreach (var permission in permissions)
                            {
                                var permissionValue = $"{permission.Resource}:{permission.Action}";
                                identity.AddClaim(new Claim("permission", permissionValue));
                            }
                            logger.LogDebug("Added {Count} permission claims to user {UserId}", permissions.Count, userId);
                        }
                        else
                        {
                            logger.LogDebug("No permissions found for user {UserId}", userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to enrich user claims with RPT permissions");
                // Continue without enrichment
            }
        }
        else
        {
            logger.LogDebug("User is NOT authenticated");
        }

        await next(context);
    }
}