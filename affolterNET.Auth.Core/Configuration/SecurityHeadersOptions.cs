namespace affolterNET.Auth.Core.Configuration;

/// <summary>
/// Security headers configuration options
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:SecurityHeaders";

    /// <summary>
    /// Whether to enable security headers (default: true)
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Whether running in development mode (affects CSP strictness)
    /// </summary>
    public bool IsDevelopment { get; set; } = false;
    
    /// <summary>
    /// Identity provider host for Content Security Policy form-action directive
    /// </summary>
    public string IdpHost { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional allowed hosts for connect-src directive (API endpoints, WebSocket, etc.)
    /// </summary>
    public List<string> AllowedConnectSources { get; set; } = new();
    
    /// <summary>
    /// Additional allowed hosts for script-src directive
    /// </summary>
    public List<string> AllowedScriptSources { get; set; } = new();
    
    /// <summary>
    /// Additional allowed hosts for style-src directive
    /// </summary>
    public List<string> AllowedStyleSources { get; set; } = new();
    
    /// <summary>
    /// Additional allowed hosts for img-src directive
    /// </summary>
    public List<string> AllowedImageSources { get; set; } = new();
    
    /// <summary>
    /// Whether to remove the Server header (default: true)
    /// </summary>
    public bool RemoveServerHeader { get; set; } = true;
    
    /// <summary>
    /// HSTS max age in seconds (default: 1 year)
    /// </summary>
    public int HstsMaxAge { get; set; } = 31536000; // 1 year
    
    /// <summary>
    /// Whether to include subdomains in HSTS (default: true)
    /// </summary>
    public bool HstsIncludeSubDomains { get; set; } = true;
    
    /// <summary>
    /// Whether to enable HSTS preload (default: false)
    /// </summary>
    public bool HstsPreload { get; set; } = false;
    
    /// <summary>
    /// Custom CSP directives as key-value pairs
    /// </summary>
    public Dictionary<string, string> CustomCspDirectives { get; set; } = new();

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>SecurityHeadersOptions with environment-appropriate defaults</returns>
    public static SecurityHeadersOptions CreateDefaults(bool isDevelopment = false)
    {
        return new SecurityHeadersOptions
        {
            Enabled = true,
            IsDevelopment = isDevelopment,
            IdpHost = string.Empty,
            AllowedConnectSources = new List<string>(),
            AllowedScriptSources = new List<string>(),
            AllowedStyleSources = new List<string>(),
            AllowedImageSources = new List<string>(),
            RemoveServerHeader = true,
            HstsMaxAge = isDevelopment ? 0 : 31536000, // Disable HSTS in development
            HstsIncludeSubDomains = !isDevelopment, // More relaxed in development
            HstsPreload = false,
            CustomCspDirectives = new Dictionary<string, string>()
        };
    }
}