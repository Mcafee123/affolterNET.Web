using Microsoft.Extensions.Configuration;

namespace affolterNET.Auth.Core.Configuration;

/// <summary>
/// Unified authentication configuration combining OIDC and Auth Core settings
/// </summary>
public class AuthConfiguration
{
    public const string SectionName = "Auth";
    
    public static AuthConfiguration Bind(IConfiguration config)
    {
        var authConfig = new AuthConfiguration();
        config.GetSection(SectionName).Bind(authConfig);
        return authConfig;
    }
    
    #region Core Authentication Properties
    
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
    
    #endregion
    
    #region OIDC Flow Configuration
    
    /// <summary>
    /// OIDC response type (default: "code")
    /// </summary>
    public string ResponseType { get; set; } = "code";

    public string GetResponseType(string defaultValue = "code")
    {
        return string.IsNullOrEmpty(ResponseType) ? defaultValue : ResponseType;
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
    /// OIDC scopes (space-separated)
    /// </summary>
    public string Scopes { get; set; } = "openid profile email";

    public string GetScopes(string defaultValue = "openid profile email")
    {
        return string.IsNullOrEmpty(Scopes) ? defaultValue : Scopes;
    }
    
    /// <summary>
    /// Redirect URI for OIDC flows
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    public string GetRedirectUri()
    {
        return RedirectUri;
    }

    /// <summary>
    /// Authentication cookie name
    /// </summary>
    public string CookieName { get; set; } = ".AspNetCore.Cookies";
    
    #endregion
    
    #region Cache Configuration
    
    /// <summary>
    /// Cache configuration options
    /// </summary>
    public CacheOptions Cache { get; set; } = new();
    
    #endregion
    
    #region RPT Configuration
    
    /// <summary>
    /// Request Party Token (RPT) configuration options
    /// </summary>
    public RptOptions Rpt { get; set; } = new();
    
    #endregion
}

/// <summary>
/// Caching configuration options
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Default cache expiration time
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
    
    /// <summary>
    /// Permission cache expiration time
    /// </summary>
    public TimeSpan PermissionCacheExpiration { get; set; } = TimeSpan.FromMinutes(10);
    
    /// <summary>
    /// Maximum number of cached items
    /// </summary>
    public int MaxCacheSize { get; set; } = 1000;
}

/// <summary>
/// Request Party Token (RPT) configuration options
/// </summary>
public class RptOptions
{
    /// <summary>
    /// RPT token endpoint template
    /// </summary>
    public string Endpoint { get; set; } = "/realms/{realm}/protocol/openid_connect/token";
    
    /// <summary>
    /// RPT audience (usually same as ClientId)
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to enable RPT token caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>
    /// RPT token cache expiration time
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(10);
}

// Extension methods for string URL manipulation
public static class StringExtensions
{
    /// <summary>
    /// Combines URL segments with proper slash handling
    /// </summary>
    public static string UrlCombine(this string baseUrl, params string[] segments)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

        var url = baseUrl.TrimEnd('/');

        foreach (var segment in segments.Where(s => !string.IsNullOrEmpty(s)))
        {
            url = url + "/" + segment.TrimStart('/');
        }

        return url;
    }
}