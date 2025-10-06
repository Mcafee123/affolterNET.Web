using affolterNET.Web.Core.Options;
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
        options.FrontendUrl = FrontendUrl;
        options.EnableApiNotFound = EnableApiNotFound;
        options.EnableAntiforgery = EnableAntiforgery;
        options.EnableHttpsRedirection = EnableHttpsRedirection;
        options.EnableNoUnauthorizedRedirect = EnableNoUnauthorizedRedirect;
        options.EnableRptTokens = EnableRptTokens;
        options.EnableStaticFiles = EnableStaticFiles;
        options.EnableTokenRefresh = EnableTokenRefresh;
        options.ErrorPath = ErrorPath;
        options.FallbackPage = FallbackPage;
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
    }

    /// <summary>
    /// Backend URL - url of the BFF application
    /// </summary>
    public string BackendUrl { get; set; }
    
    /// <summary>
    /// Frontend URL (same as backend url for production build)
    /// </summary>
    public string FrontendUrl { get; set; }
    
    /// <summary>
    /// API route prefix for handling API-specific behavior
    /// </summary>
    public string[] ApiRoutePrefixes { get; set; }

    /// <summary>
    /// Authentication mode for the BFF (default: None)
    /// </summary>
    public AuthenticationMode AuthMode { get; private set; }

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
    /// Whether to enable static files
    /// </summary>
    public bool EnableStaticFiles { get; set; }

    /// <summary>
    /// Whether to enable token refresh middleware
    /// </summary>
    public bool EnableTokenRefresh { get; set; }

    /// <summary>
    /// Error page path
    /// </summary>
    public string ErrorPath { get; set; }

    /// <summary>
    /// Fallback page for SPA routing
    /// </summary>
    public string? FallbackPage { get; set; }
}