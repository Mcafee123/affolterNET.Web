using Microsoft.AspNetCore.Authorization;

namespace affolterNET.Web.Core.Authorization;

/// <summary>
/// Authorization attribute that requires specific roles for accessing controllers or actions
/// </summary>
public class RequireRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initializes a new instance of the RequireRoleAttribute with the specified roles
    /// </summary>
    /// <param name="roles">The roles required to access the resource</param>
    public RequireRoleAttribute(params string[] roles)
    {
        if (roles == null || roles.Length == 0)
        {
            throw new ArgumentException("At least one role must be specified", nameof(roles));
        }

        // Set the roles directly - this uses built-in ASP.NET Core role authorization
        Roles = string.Join(",", roles);
    }
}