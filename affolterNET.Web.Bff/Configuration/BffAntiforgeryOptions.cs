using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;
using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Bff.Configuration;

/// <summary>
/// Antiforgery token configuration options
/// </summary>
public class BffAntiforgeryOptions: IConfigurableOptions<BffAntiforgeryOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET:Web:Bff:Antiforgery";

    public static BffAntiforgeryOptions CreateDefaults(AppSettings settings)
    {
        return new BffAntiforgeryOptions(settings);
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
    public BffAntiforgeryOptions() : this(new AppSettings())
    {
    }
    
    /// <summary>
    /// Constructor with BffAppSettings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing environment and authentication mode</param>
    private BffAntiforgeryOptions(AppSettings settings)
    {
        ServerCookieName = "__Host-X-XSRF-TOKEN";
        ClientCookieName = "X-XSRF-TOKEN";
        HeaderName = "X-XSRF-TOKEN";
        CookiePath = "/";
        SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        RequireSecure = true; // Required for __Host- prefix
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
}