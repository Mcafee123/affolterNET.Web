namespace affolterNET.Auth.Bff.Configuration;

/// <summary>
/// Antiforgery token configuration options
/// </summary>
public class AntiforgeryOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Bff:Antiforgery";

    /// <summary>
    /// Server-side antiforgery cookie name (secure, HttpOnly)
    /// </summary>
    public string ServerCookieName { get; set; } = "__Host-X-XSRF-TOKEN";
    
    /// <summary>
    /// Client-side antiforgery cookie name (accessible to JavaScript)
    /// </summary>
    public string ClientCookieName { get; set; } = "X-XSRF-TOKEN";
    
    /// <summary>
    /// HTTP header name for antiforgery token
    /// </summary>
    public string HeaderName { get; set; } = "X-XSRF-TOKEN";
    
    /// <summary>
    /// Cookie path for antiforgery cookies
    /// </summary>
    public string CookiePath { get; set; } = "/";
    
    /// <summary>
    /// SameSite mode for antiforgery cookies
    /// </summary>
    public Microsoft.AspNetCore.Http.SameSiteMode SameSiteMode { get; set; } = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    
    /// <summary>
    /// Whether to require secure cookies (HTTPS)
    /// </summary>
    public bool RequireSecure { get; set; } = true;

    /// <summary>
    /// Creates default configuration for the specified environment
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>AntiforgeryOptions with environment-appropriate defaults</returns>
    public static AntiforgeryOptions CreateDefaults(bool isDevelopment = false)
    {
        return new AntiforgeryOptions
        {
            ServerCookieName = "__Host-X-XSRF-TOKEN",
            ClientCookieName = "X-XSRF-TOKEN",
            HeaderName = "X-XSRF-TOKEN",
            CookiePath = "/",
            SameSiteMode = isDevelopment ? Microsoft.AspNetCore.Http.SameSiteMode.Lax : Microsoft.AspNetCore.Http.SameSiteMode.Strict,
            RequireSecure = !isDevelopment // Allow non-secure cookies in development
        };
    }
}