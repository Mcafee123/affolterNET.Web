using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using affolterNET.Web.Core.Services;
using affolterNET.Web.Core.Models;
using System.Security.Claims;

namespace affolterNET.Web.Bff.Controllers;

[ApiController]
[Route("bff/[controller]")]
public class UserController(IClaimsEnrichmentService claimsEnrichmentService) : ControllerBase
{
    /// <summary>
    /// Gets current user information - supports both authenticated and anonymous users
    /// This endpoint replaces the DataFlow.Web UserController endpoint
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        // Handle anonymous users
        if (!User.Identity?.IsAuthenticated == true)
        {
            return Ok(new
            {
                IsAuthenticated = false,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = "roles",
                Claims = Array.Empty<object>()
            });
        }

        // For authenticated users, return enriched user context
        var userContext = await claimsEnrichmentService.EnrichUserContextAsync(User, cancellationToken: cancellationToken);
        
        // Return in a format compatible with the original DataFlow.Web UserController
        return Ok(new
        {
            IsAuthenticated = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = "roles",
            Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToArray(),
            UserContext = userContext // Additional enriched data
        });
    }

    /// <summary>
    /// Gets detailed user information with permissions (requires authentication)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserDetailed(CancellationToken cancellationToken)
    {
        var userContext = await claimsEnrichmentService.EnrichUserContextAsync(User, cancellationToken: cancellationToken);
        return Ok(userContext);
    }

    /// <summary>
    /// Gets user permissions (requires authentication)
    /// </summary>
    [HttpGet("permissions")]
    [Authorize]
    public async Task<IActionResult> GetUserPermissions(CancellationToken cancellationToken)
    {
        var userContext = await claimsEnrichmentService.EnrichUserContextAsync(User, cancellationToken: cancellationToken);
        return Ok(userContext.Permissions);
    }
}