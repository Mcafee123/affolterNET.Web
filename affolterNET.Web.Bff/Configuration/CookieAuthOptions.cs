using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// Cookie authentication configuration options for BFF scenarios
/// </summary>
public class CookieAuthOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Web:Bff:Cookie";

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public CookieAuthOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public CookieAuthOptions(bool isDev)
    {
        Name = "__Host-bff";
        HttpOnly = true;
        Secure = !isDev; // Allow non-secure cookies in development
        SameSite = isDev ? "Lax" : "Strict"; // More relaxed in development
        ExpireTimeSpan = isDev ? TimeSpan.FromHours(4) : TimeSpan.FromHours(8); // Shorter session in development
        SlidingExpiration = true;
    }

    /// <summary>
    /// Cookie name for authentication
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Whether the cookie should be HttpOnly
    /// </summary>
    public bool HttpOnly { get; set; }

    /// <summary>
    /// Whether the cookie should be sent only over HTTPS
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// SameSite attribute for the cookie
    /// </summary>
    public string SameSite { get; set; }

    /// <summary>
    /// Cookie expiration time span
    /// </summary>
    public TimeSpan ExpireTimeSpan { get; set; }

    /// <summary>
    /// Whether to use sliding expiration
    /// </summary>
    public bool SlidingExpiration { get; set; }

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured CookieAuthOptions instance</returns>
    public static CookieAuthOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<CookieAuthOptions>? configure = null)
    {
        var options = new CookieAuthOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}