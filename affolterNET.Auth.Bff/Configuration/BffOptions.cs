namespace affolterNET.Auth.Bff.Configuration;

/// <summary>
/// Backend for Frontend (BFF) configuration options
/// </summary>
public class BffOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Bff:Options";

    /// <summary>
    /// Whether to enable session management
    /// </summary>
    public bool EnableSessionManagement { get; set; } = true;

    /// <summary>
    /// Base path for BFF management endpoints
    /// </summary>
    public string ManagementBasePath { get; set; } = "/bff";

    /// <summary>
    /// Whether to require logout session ID
    /// </summary>
    public bool RequireLogoutSessionId { get; set; } = false;

    /// <summary>
    /// Whether to revoke refresh tokens on logout
    /// </summary>
    public bool RevokeRefreshTokenOnLogout { get; set; } = true;

    /// <summary>
    /// Whether to logout all user sessions on backchannel logout
    /// </summary>
    public bool BackchannelLogoutAllUserSessions { get; set; } = false;

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>BffOptions with environment-appropriate defaults</returns>
    public static BffOptions CreateDefaults(bool isDevelopment = false)
    {
        return new BffOptions
        {
            EnableSessionManagement = true,
            ManagementBasePath = "/bff",
            RequireLogoutSessionId = false,
            RevokeRefreshTokenOnLogout = true,
            BackchannelLogoutAllUserSessions = false
        };
    }
}