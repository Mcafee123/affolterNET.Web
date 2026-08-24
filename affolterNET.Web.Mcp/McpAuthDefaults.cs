namespace affolterNET.Web.Mcp;

/// <summary>
/// Scheme and policy names used by the MCP resource-server integration.
/// They are deliberately distinct from the host application's own schemes so an
/// MCP endpoint can live inside a cookie-based BFF without disturbing it.
/// </summary>
public static class McpAuthDefaults
{
    /// <summary>
    /// Challenge scheme: answers unauthenticated MCP requests with
    /// <c>401 + WWW-Authenticate: Bearer resource_metadata="…"</c> (RFC 9728) and serves
    /// the <c>/.well-known/oauth-protected-resource/…</c> document.
    /// </summary>
    public const string AuthenticationScheme = "McpOAuth";

    /// <summary>
    /// Display name of the challenge scheme.
    /// </summary>
    public const string DisplayName = "MCP OAuth";

    /// <summary>
    /// JWT Bearer scheme that actually validates the access token. The challenge
    /// scheme forwards authentication here.
    /// </summary>
    public const string BearerScheme = "McpBearer";

    /// <summary>
    /// Authorization policy applied to MCP endpoints.
    /// </summary>
    public const string PolicyName = "McpOAuthPolicy";
}
