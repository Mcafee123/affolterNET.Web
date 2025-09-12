using Microsoft.AspNetCore.Authorization;

namespace affolterNET.Auth.Core.Authorization;

/// <summary>
/// Authorization attribute that requires specific permissions for accessing controllers or actions
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initializes a new instance of the RequirePermissionAttribute with the specified permissions
    /// </summary>
    /// <param name="permissions">The permissions required to access the resource</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));
        }

        // Set the policy to a comma-separated list of permissions
        // This will be processed by PermissionAuthorizationPolicyProvider
        Policy = string.Join(",", permissions);
    }
}