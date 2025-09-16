using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Security headers configuration options
/// </summary>
public class SecurityHeadersOptions: IConfigurableOptions<SecurityHeadersOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:SecurityHeaders";

    public static SecurityHeadersOptions CreateDefaults(AppSettings settings)
    {
        return new SecurityHeadersOptions(settings);
    }

    public void CopyTo(SecurityHeadersOptions target)
    {
        target.Enabled = Enabled;
        target.IdpHost = IdpHost;
        target.AllowedConnectSources = new List<string>(AllowedConnectSources);
        target.AllowedScriptSources = new List<string>(AllowedScriptSources);
        target.AllowedStyleSources = new List<string>(AllowedStyleSources);
        target.AllowedImageSources = new List<string>(AllowedImageSources);
        target.RemoveServerHeader = RemoveServerHeader;
        target.HstsMaxAge = HstsMaxAge;
        target.HstsIncludeSubDomains = HstsIncludeSubDomains;
        target.HstsPreload = HstsPreload;
        target.CustomCspDirectives = new Dictionary<string, string>(CustomCspDirectives);
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public SecurityHeadersOptions() : this(new AppSettings(false, AuthenticationMode.None))
    {
    }
    
    /// <summary>
    /// Constructor with settings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing development mode and authentication mode</param>
    private SecurityHeadersOptions(AppSettings settings)
    {
        Enabled = true;
        IdpHost = string.Empty;
        AllowedConnectSources = new List<string>();
        AllowedScriptSources = new List<string>();
        AllowedStyleSources = new List<string>();
        AllowedImageSources = new List<string>();
        RemoveServerHeader = true;
        HstsMaxAge = settings.IsDev ? 0 : 31536000; // Disable HSTS in development
        HstsIncludeSubDomains = !settings.IsDev; // More relaxed in development
        HstsPreload = false;
        CustomCspDirectives = new Dictionary<string, string>();
        
        // Development-specific defaults
        if (settings.IsDev)
        {
            // Allow localhost for development
            AllowedConnectSources.Add("http://localhost:*");
            AllowedConnectSources.Add("https://localhost:*");
            AllowedConnectSources.Add("ws://localhost:*");
            AllowedConnectSources.Add("wss://localhost:*");
        }
    }

    /// <summary>
    /// Whether to enable security headers (default: true)
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Identity provider host for Content Security Policy form-action directive
    /// </summary>
    public string IdpHost { get; set; }
    
    /// <summary>
    /// Additional allowed hosts for connect-src directive (API endpoints, WebSocket, etc.)
    /// </summary>
    public List<string> AllowedConnectSources { get; set; }
    
    /// <summary>
    /// Additional allowed hosts for script-src directive
    /// </summary>
    public List<string> AllowedScriptSources { get; set; }
    
    /// <summary>
    /// Additional allowed hosts for style-src directive
    /// </summary>
    public List<string> AllowedStyleSources { get; set; }
    
    /// <summary>
    /// Additional allowed hosts for img-src directive
    /// </summary>
    public List<string> AllowedImageSources { get; set; }
    
    /// <summary>
    /// Whether to remove the Server header (default: true)
    /// </summary>
    public bool RemoveServerHeader { get; set; }
    
    /// <summary>
    /// HSTS max age in seconds (default: 1 year)
    /// </summary>
    public int HstsMaxAge { get; set; }
    
    /// <summary>
    /// Whether to include subdomains in HSTS (default: true)
    /// </summary>
    public bool HstsIncludeSubDomains { get; set; }
    
    /// <summary>
    /// Whether to enable HSTS preload (default: false)
    /// </summary>
    public bool HstsPreload { get; set; }
    
    /// <summary>
    /// Custom CSP directives as key-value pairs
    /// </summary>
    public Dictionary<string, string> CustomCspDirectives { get; set; }
}