using affolterNET.Auth.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Auth.Core.Configuration;

/// <summary>
/// Legacy authentication configuration - DEPRECATED
/// Use AuthProviderOptions, OidcOptions, and specific option classes instead
/// </summary>
[Obsolete("Use AuthProviderOptions, OidcOptions, and specific option classes instead")]
public class AuthConfiguration : AuthProviderOptions
{
    public new const string SectionName = "affolterNET.Auth";
    
    public static AuthConfiguration Bind(IConfiguration config)
    {
        var authConfig = new AuthConfiguration();
        config.GetSection(SectionName).Bind(authConfig);
        return authConfig;
    }
    
    /// <summary>
    /// OIDC response type (default: "code") - DEPRECATED: Use OidcOptions.ResponseType
    /// </summary>
    [Obsolete("Use OidcOptions.ResponseType instead")]
    public string ResponseType { get; set; } = "code";

    [Obsolete("Use OidcOptions.GetResponseType() instead")]
    public string GetResponseType(string defaultValue = "code")
    {
        return string.IsNullOrEmpty(ResponseType) ? defaultValue : ResponseType;
    }

    /// <summary>
    /// OIDC callback path (default: "/signin-oidc") - DEPRECATED: Use BFF-specific options
    /// </summary>
    [Obsolete("Use BffAuthOptions.CallbackPath instead")]
    public string CallbackPath { get; set; } = "/signin-oidc";
    
    /// <summary>
    /// OIDC signout callback path (default: "/signout-callback-oidc") - DEPRECATED: Use BFF-specific options
    /// </summary>
    [Obsolete("Use BffAuthOptions.SignoutCallBack instead")]
    public string SignoutCallBack { get; set; } = "/signout-callback-oidc";
    
    /// <summary>
    /// OIDC scopes (space-separated) - DEPRECATED: Use OidcOptions.Scopes
    /// </summary>
    [Obsolete("Use OidcOptions.Scopes instead")]
    public string Scopes { get; set; } = "openid profile email";

    [Obsolete("Use OidcOptions.GetScopes() instead")]
    public string GetScopes(string defaultValue = "openid profile email")
    {
        return string.IsNullOrEmpty(Scopes) ? defaultValue : Scopes;
    }
    
    /// <summary>
    /// Redirect URI for OIDC flows - DEPRECATED: Use BFF-specific options
    /// </summary>
    [Obsolete("Use BffAuthOptions.RedirectUri instead")]
    public string RedirectUri { get; set; } = string.Empty;

    [Obsolete("Use BffAuthOptions.GetRedirectUri() instead")]
    public string GetRedirectUri()
    {
        return RedirectUri;
    }

    /// <summary>
    /// Authentication cookie name - DEPRECATED: Use BFF-specific options
    /// </summary>
    [Obsolete("Use BffAuthOptions.CookieName instead")]
    public string CookieName { get; set; } = ".AspNetCore.Cookies";
    
    /// <summary>
    /// Cache configuration options - DEPRECATED: Use PermissionCacheOptions
    /// </summary>
    [Obsolete("Use PermissionCacheOptions instead")]
    public PermissionCacheOptions Cache { get; set; } = new();
}
