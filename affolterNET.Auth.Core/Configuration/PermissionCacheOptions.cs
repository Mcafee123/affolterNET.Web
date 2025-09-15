namespace affolterNET.Auth.Core.Configuration;

/// <summary>
/// Permission caching configuration options
/// </summary>
public class PermissionCacheOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:PermissionCache";

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

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>PermissionCacheOptions with environment-appropriate defaults</returns>
    public static PermissionCacheOptions CreateDefaults(bool isDevelopment = false)
    {
        return new PermissionCacheOptions
        {
            DefaultExpiration = isDevelopment ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(15), // Shorter cache in development
            PermissionCacheExpiration = isDevelopment ? TimeSpan.FromMinutes(3) : TimeSpan.FromMinutes(10), // Shorter permission cache in development
            MaxCacheSize = isDevelopment ? 100 : 1000 // Smaller cache size in development
        };
    }
}