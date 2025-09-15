using affolterNET.Web.Bff.Configuration;
using affolterNET.Web.Bff.Models;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Bff.Options;

/// <summary>
/// Configuration options for BFF application pipeline
/// </summary>
public class BffAppOptions : CoreAppOptions
{
    public BffAppOptions(bool isDev): base(isDev)
    {
        IsDev = isDev;
        AuthorizationMode = AuthorizationMode.PermissionBased;
        AntiForgery = new BffAntiforgeryOptions(isDev);
        BffAuth = new BffAuthOptions(isDev);
        Bff = new BffOptions(isDev);
        CookieAuth = new CookieAuthOptions(isDev);
        Rpt = new RptOptions(isDev);
    }

    public bool IsDev { get; }

    /// <summary>
    /// Authorization mode for the application
    /// </summary>
    public AuthorizationMode AuthorizationMode { get; set; }

    /// <summary>
    /// Whether to enable antiforgery protection
    /// </summary>
    public bool EnableAntiforgery { get; set; } = true;
    
    public BffAntiforgeryOptions AntiForgery { get; set; }

    public BffAuthOptions BffAuth { get; set; }

    public BffOptions Bff { get; set; }

    public CookieAuthOptions CookieAuth { get; set; }

    public RptOptions Rpt { get; set; }
    

    /// <summary>
    /// Whether to enable HTTPS redirection (WARNING: dev mode not working when set to false)
    /// </summary>
    public bool EnableHttpsRedirection { get; set; } = true;

    /// <summary>
    /// Whether to enable YARP reverse proxy
    /// </summary>
    public bool EnableYarp { get; set; } = true;

    /// <summary>
    /// Whether to enable static files
    /// </summary>
    public bool EnableStaticFiles { get; set; } = true;

    /// <summary>
    /// Whether to enable token refresh middleware
    /// </summary>
    public bool EnableTokenRefresh { get; set; } = true;

    /// <summary>
    /// Whether to enable RPT tokens for permission-based auth
    /// </summary>
    public bool EnableRptTokens { get; set; } = true;

    /// <summary>
    /// Whether to enable unauthorized redirect prevention for API routes
    /// </summary>
    public bool EnableNoUnauthorizedRedirect { get; set; } = true;

    /// <summary>
    /// API route prefix for handling API-specific behavior
    /// </summary>
    public string[] ApiRoutePrefixes { get; set; } = ["/api"];

    /// <summary>
    /// Whether to enable API 404 handling
    /// </summary>
    public bool EnableApiNotFound { get; set; } = true;

    /// <summary>
    /// Whether to enable reverse proxy functionality
    /// </summary>
    public bool EnableReverseProxy { get; set; } = true;

    /// <summary>
    /// Whether to only enable reverse proxy in development
    /// </summary>
    public bool OnlyReverseProxyInDevelopment { get; set; } = true;

    /// <summary>
    /// Fallback page for SPA routing
    /// </summary>
    public string? FallbackPage { get; set; } = "/_Host";

    /// <summary>
    /// Error page path
    /// </summary>
    public string ErrorPath { get; set; } = "/Error";

    /// <summary>
    /// Configuration action for API documentation (Swagger/OpenAPI) - called after security but before routing
    /// </summary>
    public Action<IApplicationBuilder>? ConfigureApiDocumentation { get; set; }

    /// <summary>
    /// Configuration action for custom middleware - called before routing but after authentication
    /// </summary>
    public Action<IApplicationBuilder>? ConfigureCustomMiddleware { get; set; }

    public void Configure(IServiceCollection services)
    {
        services.Configure<BffAntiforgeryOptions>(options => AntiForgery.CopyTo(options));      
    }
}