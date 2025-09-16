using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// Cookie authentication configuration options for BFF scenarios
/// </summary>
public class CookieAuthOptions: IConfigurableOptions<CookieAuthOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Cookie";

    public static CookieAuthOptions CreateDefaults(bool isDev)
    {
        return new CookieAuthOptions(isDev);
    }

    public void CopyTo(CookieAuthOptions target)
    {
        target.ExpireTimeSpan = ExpireTimeSpan;
        target.HttpOnly = HttpOnly;
        target.Name = Name;
        target.SameSite = SameSite;
        target.Secure = Secure;
        target.SlidingExpiration = SlidingExpiration;
    }

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
    private CookieAuthOptions(bool isDev)
    {
        Name = "__Host-bff";
        HttpOnly = true;
        Secure = !isDev; // Allow non-secure cookies in development
        SameSite = isDev ? "Lax" : "Strict"; // More relaxed in development
        ExpireTimeSpan = isDev ? TimeSpan.FromHours(4) : TimeSpan.FromHours(8); // Shorter session in development
        SlidingExpiration = true;
    }

    /// <summary>
    /// Cookie expiration time span
    /// </summary>
    public TimeSpan ExpireTimeSpan { get; set; }

    /// <summary>
    /// Whether the cookie should be HttpOnly
    /// </summary>
    public bool HttpOnly { get; set; }

    /// <summary>
    /// Cookie name for authentication
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// SameSite attribute for the cookie
    /// </summary>
    public string SameSite { get; set; }

    /// <summary>
    /// Whether the cookie should be sent only over HTTPS
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// Whether to use sliding expiration
    /// </summary>
    public bool SlidingExpiration { get; set; }
}