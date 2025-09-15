using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using affolterNET.Web.Bff.Services;

namespace affolterNET.Web.Bff.Controllers;

[ApiController]
[Route("bff/account")]
[IgnoreAntiforgeryToken]
public class BffController(IBffSessionService sessionService) : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl = null, [FromQuery] string? claimsChallenge = null)
    {
        var redirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri
        };
        
        // Support claims challenge for conditional access scenarios
        if (!string.IsNullOrEmpty(claimsChallenge))
        {
            string jsonString = claimsChallenge.Replace("\\", "")
                .Trim(new char[1] { '"' });
            properties.Items["claims"] = jsonString;
        }
        
        return Challenge(properties);
    }

    [HttpPost("logout")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        await sessionService.RevokeTokensAsync(HttpContext);
        
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }
}