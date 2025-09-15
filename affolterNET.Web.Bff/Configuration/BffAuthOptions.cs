using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// BFF authentication configuration options combining Core auth provider settings with BFF-specific settings
/// </summary>
public class BffAuthOptions : AuthProviderOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public new const string SectionName = "affolterNET.Web:Bff";

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public BffAuthOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public BffAuthOptions(bool isDev) : base(isDev)
    {
        CallbackPath = "/signin-oidc";
        SignoutCallBack = "/signout-callback-oidc";
        PostLogoutRedirectUri = "/signout-callback-oidc";
        RedirectUri = string.Empty;
        CookieName = ".AspNetCore.Cookies";
        Cookie = new CookieAuthOptions(isDev);
        Oidc = new OidcOptions(isDev);
        Bff = new BffOptions(isDev);
    }
    
    /// <summary>
    /// OIDC callback path (default: "/signin-oidc")
    /// </summary>
    public string CallbackPath { get; set; }
    
    /// <summary>
    /// OIDC signout callback path (default: "/signout-callback-oidc")
    /// </summary>
    public string SignoutCallBack { get; set; }
    
    /// <summary>
    /// Post-logout redirect URI for OIDC flows
    /// </summary>
    public string PostLogoutRedirectUri { get; set; }
    
    /// <summary>
    /// Redirect URI for OIDC flows
    /// </summary>
    public string RedirectUri { get; set; }

    /// <summary>
    /// Authentication cookie name
    /// </summary>
    public string CookieName { get; set; }

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
    public CookieAuthOptions Cookie { get; set; }
    
    /// <summary>
    /// OIDC protocol configuration
    /// </summary>
    public Core.Configuration.OidcOptions Oidc { get; set; }
    
    /// <summary>
    /// BFF configuration options
    /// </summary>
    public BffOptions Bff { get; set; }

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured BffAuthOptions instance</returns>
    public static BffAuthOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<BffAuthOptions>? configure = null)
    {
        var options = new BffAuthOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}