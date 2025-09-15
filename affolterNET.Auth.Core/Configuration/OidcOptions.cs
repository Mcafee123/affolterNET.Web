namespace affolterNET.Auth.Core.Configuration;

/// <summary>
/// Core OIDC protocol configuration options
/// </summary>
public class OidcOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Oidc";

    /// <summary>
    /// OIDC response type (default: "code")
    /// </summary>
    public string ResponseType { get; set; } = "code";

    /// <summary>
    /// OIDC scopes (space-separated)
    /// </summary>
    public string Scopes { get; set; } = "openid profile email";

    /// <summary>
    /// Whether to save tokens in authentication properties (default: true)
    /// </summary>
    public bool SaveTokens { get; set; } = true;

    /// <summary>
    /// Whether to use PKCE (Proof Key for Code Exchange) for enhanced security (default: true)
    /// </summary>
    public bool UsePkce { get; set; } = true;

    /// <summary>
    /// Supported response modes
    /// </summary>
    public string[] ResponseModes { get; set; } = { "query" };

    /// <summary>
    /// Whether to map inbound claims (default: false for better security)
    /// </summary>
    public bool MapInboundClaims { get; set; } = false;

    /// <summary>
    /// Whether to get claims from UserInfo endpoint (default: true)
    /// </summary>
    public bool GetClaimsFromUserInfoEndpoint { get; set; } = true;

    /// <summary>
    /// Helper method to get scopes as string with fallback
    /// </summary>
    /// <param name="defaultValue">Default scopes if none configured</param>
    /// <returns>Configured scopes or default value</returns>
    public string GetScopes(string defaultValue = "openid profile email")
    {
        return string.IsNullOrEmpty(Scopes) ? defaultValue : Scopes;
    }

    /// <summary>
    /// Helper method to get response type with fallback
    /// </summary>
    /// <param name="defaultValue">Default response type if none configured</param>
    /// <returns>Configured response type or default value</returns>
    public string GetResponseType(string defaultValue = "code")
    {
        return string.IsNullOrEmpty(ResponseType) ? defaultValue : ResponseType;
    }

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>OidcOptions with environment-appropriate defaults</returns>
    public static OidcOptions CreateDefaults(bool isDevelopment = false)
    {
        return new OidcOptions
        {
            ResponseType = "code",
            Scopes = "openid profile email",
            SaveTokens = true,
            UsePkce = true,
            ResponseModes = { "query" },
            MapInboundClaims = false,
            GetClaimsFromUserInfoEndpoint = true
        };
    }
}