using affolterNET.Web.Core.Options;
using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Core authentication configuration for Keycloak/OIDC provider settings
/// </summary>
public class AuthProviderOptions: IConfigurableOptions<AuthProviderOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET:Web:Auth:Provider";

    public static AuthProviderOptions CreateDefaults(AppSettings settings)
    {
        return new AuthProviderOptions(settings);
    }

    public void CopyTo(AuthProviderOptions target)
    {
        target.Audience = Audience;
        target.AuthorityBase = AuthorityBase;
        target.ClientId = ClientId;
        target.ClientSecret = ClientSecret;
        target.Realm = Realm;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public AuthProviderOptions() : this(new AppSettings())
    {
    }

    /// <summary>
    /// Constructor with BffAppSettings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing environment and authentication mode</param>
    private AuthProviderOptions(AppSettings settings)
    {
        AuthorityBase = string.Empty;
        Realm = string.Empty;
        ClientId = string.Empty;
        ClientSecret = string.Empty;
    }

    /// <summary>
    /// Expected audience for JWT validation (optional)
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// The full authority URL (e.g., https://keycloak.example.com/realms/myrealm)
    /// </summary>
    public string Authority => $"{AuthorityBase.TrimEnd('/')}/realms/{Realm}";

    /// <summary>
    /// Base Keycloak URL without realm (e.g., https://keycloak.example.com)
    /// </summary>
    public string AuthorityBase { get; set; }

    /// <summary>
    /// OIDC Client ID
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// OIDC Client Secret (for confidential clients)
    /// </summary>
    [Sensible]
    public string ClientSecret { get; set; }

    /// <summary>
    /// Keycloak realm name
    /// </summary>
    public string Realm { get; set; }
}