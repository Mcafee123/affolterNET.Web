using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;
using affolterNET.Web.Core.Models;

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

    public static OidcOptions CreateDefaults(AppSettings settings)
    {
        return new OidcOptions(settings);
    }

    public void CopyTo(OidcOptions target)
    {
        target.GetClaimsFromUserInfoEndpoint = GetClaimsFromUserInfoEndpoint;
        target.MapInboundClaims = MapInboundClaims;
        target.ResponseModes = ResponseModes;
        target.ResponseType = ResponseType;
        target.SaveTokens = SaveTokens;
        target.Scopes = Scopes;
        target.UsePkce = UsePkce;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public OidcOptions() : this(new AppSettings(false, AuthenticationMode.None))
    {
    }
    
    /// <summary>
    /// Constructor with AppSettings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing environment and authentication mode</param>
    private OidcOptions(AppSettings settings)
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
    /// Whether to get claims from UserInfo endpoint (default: true)
    /// </summary>
    public bool GetClaimsFromUserInfoEndpoint { get; set; }

    /// <summary>
    /// Whether to map inbound claims (default: false for better security)
    /// </summary>
    public bool MapInboundClaims { get; set; }

    /// <summary>
    /// Supported response modes
    /// </summary>
    public string[] ResponseModes { get; set; }

    /// <summary>
    /// OIDC response type (default: "code")
    /// </summary>
    public string ResponseType { get; set; }

    /// <summary>
    /// Whether to save tokens in authentication properties (default: true)
    /// </summary>
    public bool SaveTokens { get; set; }

    /// <summary>
    /// OIDC scopes (space-separated)
    /// </summary>
    public string Scopes { get; set; }

    /// <summary>
    /// Whether to use PKCE (Proof Key for Code Exchange) for enhanced security (default: true)
    /// </summary>
    public bool UsePkce { get; set; }

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