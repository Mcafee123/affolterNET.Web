using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace affolterNET.Web.Core.Authorization;

/// <summary>
/// Authorization handler for permission-based requirements
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value.ToLower());

        if (requirement.Permissions.Any(permission => userPermissions.Contains(permission.ToLower())))
        {
            context.Succeed(requirement);
        }
        else
        {
            var missingPermissions = string.Join(", ", requirement.Permissions);
            _logger.LogDebug("None of these permissions are set for \"{user}\": {missingPermissions}", 
                context.User.Identity?.Name, missingPermissions);
            context.Fail(new AuthorizationFailureReason(this, 
                $"None of these permissions are set for \"{context.User.Identity?.Name}\": {missingPermissions}"));
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization requirement that validates user permissions
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }

    public PermissionRequirement(params string[] permissions)
    {
        Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
    }
}