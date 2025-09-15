namespace affolterNET.Auth.Bff.Configuration;

/// <summary>
/// Request Party Token (RPT) configuration options
/// </summary>
public class RptOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Bff:Rpt";

    /// <summary>
    /// RPT token endpoint template
    /// </summary>
    public string Endpoint { get; set; } = "/realms/{realm}/protocol/openid_connect/token";
    
    /// <summary>
    /// RPT audience (usually same as ClientId)
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to enable RPT token caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>
    /// RPT token cache expiration time
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>RptOptions with environment-appropriate defaults</returns>
    public static RptOptions CreateDefaults(bool isDevelopment = false)
    {
        return new RptOptions
        {
            Endpoint = "/realms/{realm}/protocol/openid_connect/token",
            Audience = string.Empty,
            EnableCaching = true,
            CacheExpiration = isDevelopment ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(10) // Shorter cache in development
        };
    }
}