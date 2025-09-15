using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Permission caching configuration options
/// </summary>
public class PermissionCacheOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Web:PermissionCache";

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public PermissionCacheOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public PermissionCacheOptions(bool isDev)
    {
        DefaultExpiration = isDev ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(15); // Shorter cache in development
        PermissionCacheExpiration = isDev ? TimeSpan.FromMinutes(3) : TimeSpan.FromMinutes(10); // Shorter permission cache in development
        MaxCacheSize = isDev ? 100 : 1000; // Smaller cache size in development
    }

    /// <summary>
    /// Default cache expiration time
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; }
    
    /// <summary>
    /// Permission cache expiration time
    /// </summary>
    public TimeSpan PermissionCacheExpiration { get; set; }
    
    /// <summary>
    /// Maximum number of cached items
    /// </summary>
    public int MaxCacheSize { get; set; }

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured PermissionCacheOptions instance</returns>
    public static PermissionCacheOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<PermissionCacheOptions>? configure = null)
    {
        var options = new PermissionCacheOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}