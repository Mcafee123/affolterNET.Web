using affolterNET.Web.Core.Options;
using Microsoft.AspNetCore.Builder;
using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// Backend for Frontend (BFF) configuration options
/// </summary>
public class BffOptions: IConfigurableOptions<BffOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET:Web:BffOptions";

    public static BffOptions CreateDefaults(AppSettings settings)
    {
        return new BffOptions(settings);
    }

    public void CopyTo(BffOptions options)
    {
        options.ApiRoutePrefixes = ApiRoutePrefixes;
        options.AuthMode = AuthMode;
        options.BackendUrl = BackendUrl;
        // options.BackchannelLogoutAllUserSessions = BackchannelLogoutAllUserSessions;
        options.ConfigureCustomMiddleware = ConfigureCustomMiddleware;
        options.EnableApiNotFound = EnableApiNotFound;
        options.EnableAntiforgery = EnableAntiforgery;
        options.EnableHttpsRedirection = EnableHttpsRedirection;
        options.EnableNoUnauthorizedRedirect = EnableNoUnauthorizedRedirect;
        options.EnableRptTokens = EnableRptTokens;
        // options.EnableSessionManagement = EnableSessionManagement;
        options.EnableStaticFiles = EnableStaticFiles;
        options.EnableTokenRefresh = EnableTokenRefresh;
        // options.EnableYarp = EnableYarp;
        options.ErrorPath = ErrorPath;
        options.FallbackPage = FallbackPage;
        options.UiDevServerUrl = UiDevServerUrl;
        // options.ManagementBasePath = ManagementBasePath;
        // options.RequireLogoutSessionId = RequireLogoutSessionId;
        // options.RevokeRefreshTokenOnLogout = RevokeRefreshTokenOnLogout;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public BffOptions() : this(new AppSettings())
    {
    }
    
    /// <summary>
    /// Constructor with BffAppSettings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing environment and authentication mode</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private BffOptions(AppSettings settings)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        AuthMode = settings.AuthMode;
        BackendUrl = "";        
        ApiRoutePrefixes = ["/api", "/bff"];
        EnableApiNotFound = true;
        EnableAntiforgery = true;
        EnableHttpsRedirection = true;
        EnableNoUnauthorizedRedirect = true;
        EnableRptTokens = true;
        EnableStaticFiles = true;
        EnableTokenRefresh = true;
        ErrorPath = "/Error";
        FallbackPage = "/_Host";
        // EnableSessionManagement = true;
        // ManagementBasePath = "/bff";
        // RequireLogoutSessionId = false;
        // RevokeRefreshTokenOnLogout = true;
        // BackchannelLogoutAllUserSessions = false;
    }

    public string BackendUrl { get; set; }
    
    /// <summary>
    /// API route prefix for handling API-specific behavior
    /// </summary>
    public string[] ApiRoutePrefixes { get; set; }

    /// <summary>
    /// Authentication mode for the BFF (default: None)
    /// </summary>
    public AuthenticationMode AuthMode { get; private set; }

    /// <summary>
    /// Whether to logout all user sessions on backchannel logout
    /// </summary>
    [Obsolete("not used at the moment")]
    public bool BackchannelLogoutAllUserSessions { get; set; }

    /// <summary>
    /// Configuration action for custom middleware - called before routing but after authentication
    /// </summary>
    public Action<IApplicationBuilder>? ConfigureCustomMiddleware { get; set; }

    /// <summary>
    /// Whether to enable API 404 handling
    /// </summary>
    public bool EnableApiNotFound { get; set; }

    /// <summary>
    /// Whether to enable unauthorized redirect prevention for API routes
    /// </summary>
    public bool EnableAntiforgery { get; set; }

    /// <summary>
    /// Whether to enable HTTPS redirection (WARNING: dev mode not working when set to false)
    /// </summary>
    public bool EnableHttpsRedirection { get; set; }

    /// <summary>
    /// Whether to enable unauthorized redirect prevention for API routes
    /// </summary>
    public bool EnableNoUnauthorizedRedirect { get; set; }

    /// <summary>
    /// Whether to enable RPT tokens for permission-based auth
    /// </summary>
    public bool EnableRptTokens { get; set; }

    /// <summary>
    /// Whether to enable session management
    /// </summary>
    [Obsolete("not used at the moment")]
    public bool EnableSessionManagement { get; set; }

    /// <summary>
    /// Whether to enable static files
    /// </summary>
    public bool EnableStaticFiles { get; set; }

    /// <summary>
    /// Whether to enable token refresh middleware
    /// </summary>
    public bool EnableTokenRefresh { get; set; }

    /// <summary>
    /// Whether to enable YARP reverse proxy
    /// </summary>
    [Obsolete("not used at the moment")]
    public bool EnableYarp { get; set; }

    /// <summary>
    /// Error page path
    /// </summary>
    public string ErrorPath { get; set; }

    /// <summary>
    /// Fallback page for SPA routing
    /// </summary>
    public string? FallbackPage { get; set; }

    /// <summary>
    /// Base path for BFF management endpoints
    /// </summary>
    [Obsolete("not used at the moment")]
    public string ManagementBasePath { get; set; }
    
    /// <summary>
    /// Whether to require logout session ID
    /// </summary>
    [Obsolete("not used at the moment")]
    public bool RequireLogoutSessionId { get; set; }

    /// <summary>
    /// Whether to revoke refresh tokens on logout
    /// </summary>
    [Obsolete("not used at the moment")]
    public bool RevokeRefreshTokenOnLogout { get; set; }
    
    /// <summary>
    /// UI development server URL for CSP configuration (e.g., "https://localhost:5173")
    /// Used to automatically configure Content Security Policy for frontend dev servers
    /// </summary>
    public string UiDevServerUrl { get; set; }
}