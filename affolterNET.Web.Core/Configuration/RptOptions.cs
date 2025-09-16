using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;
using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Request Party Token (RPT) configuration options
/// </summary>
public class RptOptions: IConfigurableOptions<RptOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Bff:Rpt";

    public static RptOptions CreateDefaults(AppSettings settings)
    {
        return new RptOptions(settings);
    }

    public void CopyTo(RptOptions options)
    {
        options.Endpoint = Endpoint;
        options.Audience = Audience;
        options.EnableCaching = EnableCaching;
        options.CacheExpiration = CacheExpiration;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public RptOptions() : this(new AppSettings(false, AuthenticationMode.None))
    {
    }
    
    /// <summary>
    /// Constructor with settings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing development mode and authentication mode</param>
    private RptOptions(AppSettings settings)
    {
        Endpoint = "/realms/{realm}/protocol/openid_connect/token";
        Audience = string.Empty;
        EnableCaching = true;
        CacheExpiration = settings.IsDev ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(10); // Shorter cache in development
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
}