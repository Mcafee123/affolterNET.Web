using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleBff.Controllers;

/// <summary>
/// Protected API endpoints - requires authentication (Authenticate mode)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProtectedController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "Protected endpoint - authentication required",
            user = User.Identity?.Name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var email = User.FindFirst("email")?.Value ?? User.FindFirst("preferred_username")?.Value;
        var name = User.FindFirst("name")?.Value ?? User.FindFirst("given_name")?.Value;

        return Ok(new
        {
            message = "User profile from authenticated endpoint",
            profile = new
            {
                email,
                name,
                subject = User.FindFirst("sub")?.Value
            },
            timestamp = DateTime.UtcNow
        });
    }
}
