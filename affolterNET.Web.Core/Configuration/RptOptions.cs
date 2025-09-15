using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Request Party Token (RPT) configuration options
/// </summary>
public class RptOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Web:Bff:Rpt";

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public RptOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public RptOptions(bool isDev)
    {
        Endpoint = "/realms/{realm}/protocol/openid_connect/token";
        Audience = string.Empty;
        EnableCaching = true;
        CacheExpiration = isDev ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(10); // Shorter cache in development
    }

    /// <summary>
    /// RPT token endpoint template
    /// </summary>
    public string Endpoint { get; set; }
    
    /// <summary>
    /// RPT audience (usually same as ClientId)
    /// </summary>
    public string Audience { get; set; }
    
    /// <summary>
    /// Whether to enable RPT token caching
    /// </summary>
    public bool EnableCaching { get; set; }
    
    /// <summary>
    /// RPT token cache expiration time
    /// </summary>
    public TimeSpan CacheExpiration { get; set; }

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured RptOptions instance</returns>
    public static RptOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<RptOptions>? configure = null)
    {
        var options = new RptOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}