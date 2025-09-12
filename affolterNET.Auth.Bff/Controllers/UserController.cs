using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using affolterNET.Auth.Core.Services;

namespace affolterNET.Auth.Bff.Controllers;

[Authorize]
[ApiController]
[Route("bff/[controller]")]
public class UserController : ControllerBase
{
    private readonly IClaimsEnrichmentService _claimsEnrichmentService;

    public UserController(IClaimsEnrichmentService claimsEnrichmentService)
    {
        _claimsEnrichmentService = claimsEnrichmentService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userContext = await _claimsEnrichmentService.EnrichUserContextAsync(User, cancellationToken: cancellationToken);
        return Ok(userContext);
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetUserPermissions(CancellationToken cancellationToken)
    {
        var userContext = await _claimsEnrichmentService.EnrichUserContextAsync(User, cancellationToken: cancellationToken);
        return Ok(userContext.Permissions);
    }
}