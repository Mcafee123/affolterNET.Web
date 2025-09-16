using affolterNET.Web.Bff.Models;
using affolterNET.Web.Core.Options;
using Microsoft.AspNetCore.Builder;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// Backend for Frontend (BFF) configuration options
/// </summary>
public class BffOptions: IConfigurableOptions<BffOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Bff:Options";

    public static BffOptions CreateDefaults(bool isDev)
    {
        return new BffOptions(isDev);
    }

    public void CopyTo(BffOptions options)
    {
        options.ApiRoutePrefixes = ApiRoutePrefixes;
        options.AuthMode = AuthMode;
        options.BackchannelLogoutAllUserSessions = BackchannelLogoutAllUserSessions;
        options.CallbackPath = CallbackPath;
        options.ConfigureCustomMiddleware = ConfigureCustomMiddleware;
        options.EnableApiNotFound = EnableApiNotFound;
        options.EnableAntiforgery = EnableAntiforgery;
        options.EnableHttpsRedirection = EnableHttpsRedirection;
        options.EnableNoUnauthorizedRedirect = EnableNoUnauthorizedRedirect;
        options.EnableRptTokens = EnableRptTokens;
        options.EnableSessionManagement = EnableSessionManagement;
        options.EnableStaticFiles = EnableStaticFiles;
        options.EnableTokenRefresh = EnableTokenRefresh;
        options.EnableYarp = EnableYarp;
        options.ErrorPath = ErrorPath;
        options.FallbackPage = FallbackPage;
        options.ManagementBasePath = ManagementBasePath;
        options.PostLogoutRedirectUri = PostLogoutRedirectUri;
        options.RedirectUri = RedirectUri;
        options.RequireLogoutSessionId = RequireLogoutSessionId;
        options.RevokeRefreshTokenOnLogout = RevokeRefreshTokenOnLogout;
        options.SignoutCallBack = SignoutCallBack;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public BffOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    private BffOptions(bool isDev)
    {
        CallbackPath = "/signin-oidc";
        SignoutCallBack = "/signout-callback-oidc";
        PostLogoutRedirectUri = "/signout-callback-oidc";
        RedirectUri = string.Empty;
        
        EnableSessionManagement = true;
        ManagementBasePath = "/bff";
        RequireLogoutSessionId = false;
        RevokeRefreshTokenOnLogout = true;
        BackchannelLogoutAllUserSessions = false;
    }
    
    /// <summary>
    /// API route prefix for handling API-specific behavior
    /// </summary>
    public string[] ApiRoutePrefixes { get; set; } = ["/api"];

    /// <summary>
    /// Authentication mode for the BFF (default: Cookie)
    /// </summary>
    public AuthorizationMode AuthMode { get; set; }

    /// <summary>
    /// Whether to logout all user sessions on backchannel logout
    /// </summary>
    public bool BackchannelLogoutAllUserSessions { get; set; }

    /// <summary>
    /// OIDC callback path (default: "/signin-oidc")
    /// </summary>
    public string CallbackPath { get; set; }

    /// <summary>
    /// Configuration action for custom middleware - called before routing but after authentication
    /// </summary>
    public Action<IApplicationBuilder>? ConfigureCustomMiddleware { get; set; }

    /// <summary>
    /// Whether to enable API 404 handling
    /// </summary>
    public bool EnableApiNotFound { get; set; } = true;

    /// <summary>
    /// Whether to enable unauthorized redirect prevention for API routes
    /// </summary>
    public bool EnableAntiforgery { get; set; } = true;

    /// <summary>
    /// Whether to enable HTTPS redirection (WARNING: dev mode not working when set to false)
    /// </summary>
    public bool EnableHttpsRedirection { get; set; } = true;

    /// <summary>
    /// Whether to enable unauthorized redirect prevention for API routes
    /// </summary>
    public bool EnableNoUnauthorizedRedirect { get; set; } = true;

    /// <summary>
    /// Whether to enable RPT tokens for permission-based auth
    /// </summary>
    public bool EnableRptTokens { get; set; } = true;

    /// <summary>
    /// Whether to enable session management
    /// </summary>
    public bool EnableSessionManagement { get; set; }

    /// <summary>
    /// Whether to enable static files
    /// </summary>
    public bool EnableStaticFiles { get; set; } = true;

    /// <summary>
    /// Whether to enable token refresh middleware
    /// </summary>
    public bool EnableTokenRefresh { get; set; } = true;

    /// <summary>
    /// Whether to enable YARP reverse proxy
    /// </summary>
    public bool EnableYarp { get; set; } = true;

    /// <summary>
    /// Error page path
    /// </summary>
    public string ErrorPath { get; set; } = "/Error";

    /// <summary>
    /// Fallback page for SPA routing
    /// </summary>
    public string? FallbackPage { get; set; } = "/_Host";

    /// <summary>
    /// Base path for BFF management endpoints
    /// </summary>
    public string ManagementBasePath { get; set; }

    /// <summary>
    /// Post-logout redirect URI for OIDC flows
    /// </summary>
    public string PostLogoutRedirectUri { get; set; }

    /// <summary>
    /// Redirect URI for OIDC flows
    /// </summary>
    public string RedirectUri { get; set; }

    /// <summary>
    /// Whether to require logout session ID
    /// </summary>
    public bool RequireLogoutSessionId { get; set; }

    /// <summary>
    /// Whether to revoke refresh tokens on logout
    /// </summary>
    public bool RevokeRefreshTokenOnLogout { get; set; }

    /// <summary>
    /// OIDC signout callback path (default: "/signout-callback-oidc")
    /// </summary>
    public string SignoutCallBack { get; set; }
}