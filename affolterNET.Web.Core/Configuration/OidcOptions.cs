using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Core OIDC protocol configuration options
/// </summary>
public class OidcOptions: IConfigurableOptions<OidcOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Oidc";

    public static OidcOptions CreateDefaults(bool isDev)
    {
        return new OidcOptions(isDev);
    }

    public void CopyTo(OidcOptions target)
    {
        target.ResponseType = ResponseType;
        target.Scopes = Scopes;
        target.SaveTokens = SaveTokens;
        target.UsePkce = UsePkce;
        target.ResponseModes = ResponseModes;
        target.MapInboundClaims = MapInboundClaims;
        target.GetClaimsFromUserInfoEndpoint = GetClaimsFromUserInfoEndpoint;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public OidcOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    private OidcOptions(bool isDev)
    {
        ResponseType = "code";
        Scopes = "openid profile email";
        SaveTokens = true;
        UsePkce = true;
        ResponseModes = new[] { "query" };
        MapInboundClaims = false;
        GetClaimsFromUserInfoEndpoint = true;
    }

    /// <summary>
    /// OIDC response type (default: "code")
    /// </summary>
    public string ResponseType { get; set; }

    /// <summary>
    /// OIDC scopes (space-separated)
    /// </summary>
    public string Scopes { get; set; }

    /// <summary>
    /// Whether to save tokens in authentication properties (default: true)
    /// </summary>
    public bool SaveTokens { get; set; }

    /// <summary>
    /// Whether to use PKCE (Proof Key for Code Exchange) for enhanced security (default: true)
    /// </summary>
    public bool UsePkce { get; set; }

    /// <summary>
    /// Supported response modes
    /// </summary>
    public string[] ResponseModes { get; set; }

    /// <summary>
    /// Whether to map inbound claims (default: false for better security)
    /// </summary>
    public bool MapInboundClaims { get; set; }

    /// <summary>
    /// Whether to get claims from UserInfo endpoint (default: true)
    /// </summary>
    public bool GetClaimsFromUserInfoEndpoint { get; set; }

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
}