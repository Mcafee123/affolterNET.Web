using affolterNET.Web.Core.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Controllers;

/// <summary>
/// Permission-based API endpoints - requires specific permissions (Authorize mode)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PermissionController : ControllerBase
{
    /// <summary>
    /// Admin endpoint - requires admin-resource:view permission
    /// </summary>
    [HttpGet("admin")]
    [RequirePermission("admin-resource:view")]
    public IActionResult GetAdmin()
    {
        return Ok(new
        {
            message = "Admin endpoint - requires admin-resource:view permission",
            resource = "admin-resource",
            action = "view",
            user = User.Identity?.Name,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Admin manage endpoint - requires admin-resource:manage permission
    /// </summary>
    [HttpPost("admin")]
    [RequirePermission("admin-resource:manage")]
    public IActionResult ManageAdmin([FromBody] object? data)
    {
        return Ok(new
        {
            message = "Admin management endpoint - requires admin-resource:manage permission",
            resource = "admin-resource",
            action = "manage",
            user = User.Identity?.Name,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// User read endpoint - requires user-resource:read permission
    /// </summary>
    [HttpGet("user")]
    [RequirePermission("user-resource:read")]
    public IActionResult GetUser()
    {
        return Ok(new
        {
            message = "User endpoint - requires user-resource:read permission",
            resource = "user-resource",
            action = "read",
            user = User.Identity?.Name,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// User create endpoint - requires user-resource:create permission
    /// </summary>
    [HttpPost("user")]
    [RequirePermission("user-resource:create")]
    public IActionResult CreateUser([FromBody] object? data)
    {
        return Ok(new
        {
            message = "User creation endpoint - requires user-resource:create permission",
            resource = "user-resource",
            action = "create",
            user = User.Identity?.Name,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// User update endpoint - requires user-resource:update permission
    /// </summary>
    [HttpPut("user/{id}")]
    [RequirePermission("user-resource:update")]
    public IActionResult UpdateUser(string id, [FromBody] object? data)
    {
        return Ok(new
        {
            message = $"User update endpoint - requires user-resource:update permission (id: {id})",
            resource = "user-resource",
            action = "update",
            user = User.Identity?.Name,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// User delete endpoint - requires user-resource:delete permission
    /// </summary>
    [HttpDelete("user/{id}")]
    [RequirePermission("user-resource:delete")]
    public IActionResult DeleteUser(string id)
    {
        return Ok(new
        {
            message = $"User deletion endpoint - requires user-resource:delete permission (id: {id})",
            resource = "user-resource",
            action = "delete",
            user = User.Identity?.Name,
            timestamp = DateTime.UtcNow
        });
    }
}
