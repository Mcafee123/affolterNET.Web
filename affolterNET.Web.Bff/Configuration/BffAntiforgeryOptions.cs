using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// Antiforgery token configuration options
/// </summary>
public class BffAntiforgeryOptions: IConfigurableOptions<BffAntiforgeryOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Bff:Antiforgery";

    public static Action<BffAntiforgeryOptions>? GetConfigureAction()
    {
        throw new NotImplementedException();
    }

    public void CopyTo(BffAntiforgeryOptions target)
    {
        target.ServerCookieName = ServerCookieName;
        target.ClientCookieName = ClientCookieName;
        target.HeaderName = HeaderName;
        target.CookiePath = CookiePath;
        target.SameSiteMode = SameSiteMode;
        target.RequireSecure = RequireSecure;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public BffAntiforgeryOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public BffAntiforgeryOptions(bool isDev)
    {
        ServerCookieName = "__Host-X-XSRF-TOKEN";
        ClientCookieName = "X-XSRF-TOKEN";
        HeaderName = "X-XSRF-TOKEN";
        CookiePath = "/";
        SameSiteMode = isDev ? Microsoft.AspNetCore.Http.SameSiteMode.Lax : Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        RequireSecure = !isDev; // Allow non-secure cookies in development
    }

    /// <summary>
    /// Server-side antiforgery cookie name (secure, HttpOnly)
    /// </summary>
    public string ServerCookieName { get; set; }
    
    /// <summary>
    /// Client-side antiforgery cookie name (accessible to JavaScript)
    /// </summary>
    public string ClientCookieName { get; set; }
    
    /// <summary>
    /// HTTP header name for antiforgery token
    /// </summary>
    public string HeaderName { get; set; }
    
    /// <summary>
    /// Cookie path for antiforgery cookies
    /// </summary>
    public string CookiePath { get; set; }
    
    /// <summary>
    /// SameSite mode for antiforgery cookies
    /// </summary>
    public Microsoft.AspNetCore.Http.SameSiteMode SameSiteMode { get; set; }
    
    /// <summary>
    /// Whether to require secure cookies (HTTPS)
    /// </summary>
    public bool RequireSecure { get; set; }

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured AntiforgeryOptions instance</returns>
    public static BffAntiforgeryOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<BffAntiforgeryOptions>? configure = null)
    {
        var options = new BffAntiforgeryOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}