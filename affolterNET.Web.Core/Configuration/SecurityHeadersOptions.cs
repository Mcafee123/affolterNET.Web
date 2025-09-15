using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Security headers configuration options
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Web:SecurityHeaders";

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public SecurityHeadersOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public SecurityHeadersOptions(bool isDev)
    {
        Enabled = true;
        IdpHost = string.Empty;
        AllowedConnectSources = new List<string>();
        AllowedScriptSources = new List<string>();
        AllowedStyleSources = new List<string>();
        AllowedImageSources = new List<string>();
        RemoveServerHeader = true;
        HstsMaxAge = isDev ? 0 : 31536000; // Disable HSTS in development
        HstsIncludeSubDomains = !isDev; // More relaxed in development
        HstsPreload = false;
        CustomCspDirectives = new Dictionary<string, string>();
        
        // Development-specific defaults
        if (isDev)
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
    /// Whether running in development mode (affects CSP strictness)
    /// </summary>
    public bool isDev { get; set; }
    
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

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured SecurityHeadersOptions instance</returns>
    public static SecurityHeadersOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<SecurityHeadersOptions>? configure = null)
    {
        var options = new SecurityHeadersOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}