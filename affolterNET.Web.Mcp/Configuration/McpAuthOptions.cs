using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Options;

namespace affolterNET.Web.Mcp.Configuration;

/// <summary>
/// Configuration for an MCP endpoint protected as an OAuth 2.1 resource server.
/// The server never issues tokens — it validates them and advertises the
/// authorization server through RFC 9728 protected resource metadata.
/// </summary>
public class McpAuthOptions : IConfigurableOptions<McpAuthOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET:Web:Mcp";

    public static McpAuthOptions CreateDefaults(AppSettings settings)
    {
        return new McpAuthOptions(settings);
    }

    public void CopyTo(McpAuthOptions target)
    {
        target.Enabled = Enabled;
        target.Path = Path;
        target.AuthorityBase = AuthorityBase;
        target.Realm = Realm;
        target.Audience = Audience;
        target.RequiredScopes = RequiredScopes;
        target.ScopesSupported = ScopesSupported;
        target.Resource = Resource;
        target.ResourceName = ResourceName;
        target.ResourceDocumentation = ResourceDocumentation;
        target.RequireHttpsMetadata = RequireHttpsMetadata;
        target.ClockSkew = ClockSkew;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public McpAuthOptions() : this(new AppSettings())
    {
    }

    private McpAuthOptions(AppSettings settings)
    {
        Enabled = false;
        Path = "/mcp";
        AuthorityBase = string.Empty;
        Realm = string.Empty;
        Audience = string.Empty;
        RequiredScopes = [];
        ScopesSupported = [];
        RequireHttpsMetadata = !settings.IsDev; // Allow plain HTTP Keycloak in development
        ClockSkew = TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Whether the MCP endpoint is served at all. Defaults to false so an app only
    /// exposes MCP once it has been configured deliberately.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Route the MCP transport is mapped to (default: /mcp)
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Base Keycloak URL without realm. Falls back to the app's
    /// <c>affolterNET:Web:Auth:Provider</c> settings when empty.
    /// </summary>
    public string AuthorityBase { get; set; }

    /// <summary>
    /// Keycloak realm. Falls back to the app's <c>affolterNET:Web:Auth:Provider</c>
    /// settings when empty.
    /// </summary>
    public string Realm { get; set; }

    /// <summary>
    /// The full authority URL (e.g., https://keycloak.example.com/realms/myrealm)
    /// </summary>
    public string Authority => $"{AuthorityBase.TrimEnd('/')}/realms/{Realm}";

    /// <summary>
    /// Expected <c>aud</c> claim. Keycloak does not implement RFC 8707 resource
    /// indicators, so the token is bound to this MCP server through an audience
    /// mapper on the requested scope instead — which makes this value mandatory.
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// Scopes a token must carry to reach the MCP endpoint. Empty means any valid
    /// token of the configured audience is accepted.
    /// </summary>
    public string[] RequiredScopes { get; set; }

    /// <summary>
    /// Scopes advertised in the protected resource metadata. Defaults to
    /// <see cref="RequiredScopes"/> when not set.
    /// </summary>
    public string[] ScopesSupported { get; set; }

    /// <summary>
    /// Canonical URI of this MCP server, e.g. https://emails.example.com/mcp.
    /// When empty it is derived from the incoming request, which is correct as long
    /// as forwarded headers are honoured behind the reverse proxy.
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Human-readable name in the protected resource metadata (optional)
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// Documentation URL in the protected resource metadata (optional)
    /// </summary>
    public string? ResourceDocumentation { get; set; }

    /// <summary>
    /// Whether the OIDC metadata of the authority must be fetched over HTTPS
    /// </summary>
    public bool RequireHttpsMetadata { get; set; }

    /// <summary>
    /// Allowed clock skew for token validation
    /// </summary>
    public TimeSpan ClockSkew { get; set; }

    /// <summary>
    /// Scopes advertised to clients — <see cref="ScopesSupported"/> when set,
    /// otherwise <see cref="RequiredScopes"/>.
    /// </summary>
    public string[] AdvertisedScopes => ScopesSupported.Length > 0 ? ScopesSupported : RequiredScopes;

    /// <summary>
    /// Throws when the endpoint is enabled but cannot validate tokens.
    /// </summary>
    public void Validate()
    {
        if (!Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(AuthorityBase) || string.IsNullOrWhiteSpace(Realm))
        {
            throw new InvalidOperationException(
                $"{SectionName}: AuthorityBase and Realm must be set (or inherited from {AuthProviderSection}) when the MCP endpoint is enabled.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException(
                $"{SectionName}: Audience must be set when the MCP endpoint is enabled — it is what binds an access token to this MCP server.");
        }

        if (!Path.StartsWith('/'))
        {
            throw new InvalidOperationException($"{SectionName}: Path must start with '/' (was '{Path}').");
        }
    }

    private const string AuthProviderSection = "affolterNET:Web:Auth:Provider";
}
