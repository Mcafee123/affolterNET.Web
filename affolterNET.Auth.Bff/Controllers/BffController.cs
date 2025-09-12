using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using affolterNET.Auth.Bff.Services;

namespace affolterNET.Auth.Bff.Controllers;

[ApiController]
[Route("bff")]
public class BffController : ControllerBase
{
    private readonly IBffSessionService _sessionService;

    public BffController(IBffSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        };
        
        return Challenge(properties, "oidc");
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
    {
        await _sessionService.RevokeTokensAsync(HttpContext);
        
        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        };
        
        return SignOut(properties, "oidc");
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUser(CancellationToken cancellationToken)
    {
        var userContext = await _sessionService.GetUserContextAsync(HttpContext, cancellationToken);
        if (userContext == null)
        {
            return Unauthorized();
        }

        return Ok(userContext);
    }

    [HttpGet("claims")]
    [Authorize]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray();
        return Ok(claims);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> PostLogout()
    {
        await _sessionService.RevokeTokensAsync(HttpContext);
        await HttpContext.SignOutAsync();
        return Ok();
    }
}