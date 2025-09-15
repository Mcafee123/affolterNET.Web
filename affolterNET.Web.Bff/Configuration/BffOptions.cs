using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// Backend for Frontend (BFF) configuration options
/// </summary>
public class BffOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Web:Bff:Options";

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public BffOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public BffOptions(bool isDev)
    {
        EnableSessionManagement = true;
        ManagementBasePath = "/bff";
        RequireLogoutSessionId = false;
        RevokeRefreshTokenOnLogout = true;
        BackchannelLogoutAllUserSessions = false;
    }

    /// <summary>
    /// Whether to enable session management
    /// </summary>
    public bool EnableSessionManagement { get; set; }

    /// <summary>
    /// Base path for BFF management endpoints
    /// </summary>
    public string ManagementBasePath { get; set; }

    /// <summary>
    /// Whether to require logout session ID
    /// </summary>
    public bool RequireLogoutSessionId { get; set; }

    /// <summary>
    /// Whether to revoke refresh tokens on logout
    /// </summary>
    public bool RevokeRefreshTokenOnLogout { get; set; }

    /// <summary>
    /// Whether to logout all user sessions on backchannel logout
    /// </summary>
    public bool BackchannelLogoutAllUserSessions { get; set; }

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured BffOptions instance</returns>
    public static BffOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<BffOptions>? configure = null)
    {
        var options = new BffOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}