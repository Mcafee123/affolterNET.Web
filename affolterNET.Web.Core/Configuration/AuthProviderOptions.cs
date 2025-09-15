using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Core authentication configuration for Keycloak/OIDC provider settings
/// </summary>
public class AuthProviderOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Web:Provider";

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public AuthProviderOptions() : this(false)
    {
    }

    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public AuthProviderOptions(bool isDev)
    {
        AuthorityBase = string.Empty;
        Realm = string.Empty;
        ClientId = string.Empty;
        ClientSecret = string.Empty;
    }

    /// <summary>
    /// The full authority URL (e.g., https://keycloak.example.com/realms/myrealm)
    /// </summary>
    public string Authority => $"{AuthorityBase.TrimEnd('/')}/realms/{Realm}";

    /// <summary>
    /// Base Keycloak URL without realm (e.g., https://keycloak.example.com)
    /// </summary>
    public string AuthorityBase { get; set; }

    /// <summary>
    /// Keycloak realm name
    /// </summary>
    public string Realm { get; set; }

    /// <summary>
    /// OIDC Client ID
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// OIDC Client Secret (for confidential clients)
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Expected audience for JWT validation (optional)
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured AuthProviderOptions instance</returns>
    public static AuthProviderOptions CreateFromConfiguration(IConfiguration configuration,
        bool isDev,
        Action<AuthProviderOptions>? configure = null)
    {
        var options = new AuthProviderOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}