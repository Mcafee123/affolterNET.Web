using System.Security.Claims;

namespace affolterNET.Web.Mcp;

/// <summary>
/// Reads OAuth scopes out of an access token principal.
/// </summary>
public static class ScopeClaims
{
    private static readonly char[] Separators = [' '];

    /// <summary>
    /// All scopes carried by the principal. Keycloak emits a single space-delimited
    /// <c>scope</c> claim; other providers use <c>scp</c>, sometimes repeated.
    /// </summary>
    public static IEnumerable<string> Read(ClaimsPrincipal principal) =>
        principal.FindAll(c => c.Type is "scope" or "scp")
            .SelectMany(c => c.Value.Split(Separators, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    /// True when the principal carries every required scope. No required scopes
    /// means any authenticated principal passes.
    /// </summary>
    public static bool Satisfies(ClaimsPrincipal principal, IReadOnlyCollection<string> requiredScopes)
    {
        if (requiredScopes.Count == 0)
        {
            return true;
        }

        var granted = Read(principal).ToHashSet(StringComparer.Ordinal);
        return requiredScopes.All(granted.Contains);
    }
}
