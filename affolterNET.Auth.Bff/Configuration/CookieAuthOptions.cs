namespace affolterNET.Auth.Bff.Configuration;

/// <summary>
/// Cookie authentication configuration options for BFF scenarios
/// </summary>
public class CookieAuthOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Bff:Cookie";

    /// <summary>
    /// Cookie name for authentication
    /// </summary>
    public string Name { get; set; } = "__Host-bff";

    /// <summary>
    /// Whether the cookie should be HttpOnly
    /// </summary>
    public bool HttpOnly { get; set; } = true;

    /// <summary>
    /// Whether the cookie should be sent only over HTTPS
    /// </summary>
    public bool Secure { get; set; } = true;

    /// <summary>
    /// SameSite attribute for the cookie
    /// </summary>
    public string SameSite { get; set; } = "Strict";

    /// <summary>
    /// Cookie expiration time span
    /// </summary>
    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromHours(8);

    /// <summary>
    /// Whether to use sliding expiration
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>CookieAuthOptions with environment-appropriate defaults</returns>
    public static CookieAuthOptions CreateDefaults(bool isDevelopment = false)
    {
        return new CookieAuthOptions
        {
            Name = "__Host-bff",
            HttpOnly = true,
            Secure = !isDevelopment, // Allow non-secure cookies in development
            SameSite = isDevelopment ? "Lax" : "Strict", // More relaxed in development
            ExpireTimeSpan = isDevelopment ? TimeSpan.FromHours(4) : TimeSpan.FromHours(8), // Shorter session in development
            SlidingExpiration = true
        };
    }
}