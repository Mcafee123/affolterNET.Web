namespace affolterNET.Auth.Core.Configuration;

/// <summary>
/// Core authentication configuration for Keycloak/OIDC provider settings
/// </summary>
public class AuthProviderOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Provider";
    
    /// <summary>
    /// The full authority URL (e.g., https://keycloak.example.com/realms/myrealm)
    /// </summary>
    public string Authority => AuthorityBase.UrlCombine("realms", Realm);
    
    /// <summary>
    /// Base Keycloak URL without realm (e.g., https://keycloak.example.com)
    /// </summary>
    public string AuthorityBase { get; set; } = string.Empty;
    
    /// <summary>
    /// Keycloak realm name
    /// </summary>
    public string Realm { get; set; } = string.Empty;
    
    /// <summary>
    /// OIDC Client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// OIDC Client Secret (for confidential clients)
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>AuthProviderOptions with environment-appropriate defaults</returns>
    public static AuthProviderOptions CreateDefaults(bool isDevelopment = false)
    {
        return new AuthProviderOptions
        {
            AuthorityBase = string.Empty,
            Realm = string.Empty,
            ClientId = string.Empty,
            ClientSecret = string.Empty
        };
    }
}