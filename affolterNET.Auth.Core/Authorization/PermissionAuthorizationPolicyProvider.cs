using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace affolterNET.Auth.Core.Authorization;

/// <summary>
/// Authorization policy provider that creates dynamic policies from permission strings
/// </summary>
public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // First check if there's a predefined policy with this name
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        // If no predefined policy exists, treat the policy name as a comma-separated list of permissions
        var permissions = policyName.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .Where(p => !string.IsNullOrEmpty(p))
                                   .ToArray();

        if (permissions.Length == 0)
        {
            return null;
        }

        // Create a dynamic policy based on the permissions
        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permissions))
            .Build();
    }
}