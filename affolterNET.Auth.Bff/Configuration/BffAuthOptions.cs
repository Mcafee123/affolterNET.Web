using affolterNET.Auth.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Auth.Bff.Configuration;

/// <summary>
/// BFF authentication configuration options combining Core auth provider settings with BFF-specific settings
/// </summary>
public class BffAuthOptions : AuthProviderOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Bff";

    /// <summary>
    /// Creates default configuration for BFF authentication
    /// </summary>
    /// <param name="config">Optional configuration to bind values from</param>
    /// <returns>BffAuthOptions instance with defaults applied</returns>
    public static BffAuthOptions CreateDefaults(IConfiguration? config = null)
    {
        var options = new BffAuthOptions();
        
        if (config != null)
        {
            config.GetSection(SectionName).Bind(options);
        }
        
        return options;
    }
    
    /// <summary>
    /// OIDC callback path (default: "/signin-oidc")
    /// </summary>
    public string CallbackPath { get; set; } = "/signin-oidc";
    
    /// <summary>
    /// OIDC signout callback path (default: "/signout-callback-oidc")
    /// </summary>
    public string SignoutCallBack { get; set; } = "/signout-callback-oidc";
    
    /// <summary>
    /// Post-logout redirect URI for OIDC flows
    /// </summary>
    public string PostLogoutRedirectUri { get; set; } = "/signout-callback-oidc";
    
    /// <summary>
    /// Redirect URI for OIDC flows
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Authentication cookie name
    /// </summary>
    public string CookieName { get; set; } = ".AspNetCore.Cookies";

    /// <summary>
    /// Helper method to get redirect URI
    /// </summary>
    /// <returns>Configured redirect URI</returns>
    public string GetRedirectUri()
    {
        return RedirectUri;
    }

    /// <summary>
    /// Helper method to get scopes as string with fallback
    /// </summary>
    /// <param name="defaultValue">Default scopes if none configured</param>
    /// <returns>Configured scopes or default value</returns>
    public string GetScopes(string defaultValue = "openid profile email")
    {
        return Oidc.GetScopes(defaultValue);
    }

    /// <summary>
    /// Helper method to get response type with fallback
    /// </summary>
    /// <param name="defaultValue">Default response type if none configured</param>
    /// <returns>Configured response type or default value</returns>
    public string GetResponseType(string defaultValue = "code")
    {
        return Oidc.GetResponseType(defaultValue);
    }
    
    /// <summary>
    /// Cookie authentication configuration
    /// </summary>
    public CookieAuthOptions Cookie { get; set; } = new();
    
    /// <summary>
    /// OIDC protocol configuration
    /// </summary>
    public Core.Configuration.OidcOptions Oidc { get; set; } = new();
    
    /// <summary>
    /// BFF configuration options
    /// </summary>
    public BffOptions Bff { get; set; } = new();
}