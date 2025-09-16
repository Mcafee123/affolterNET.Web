using affolterNET.Web.Bff.Configuration;
using affolterNET.Web.Bff.Models;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Bff.Options;

/// <summary>
/// Configuration options for BFF application pipeline
/// </summary>
public class BffAppOptions : CoreAppOptions
{
    public BffAppOptions(bool isDev, IConfiguration config): base(isDev, config)
    {
        IsDev = isDev;
        AntiForgery = config.CreateFromConfig<BffAntiforgeryOptions>(isDev);
        Bff = config.CreateFromConfig<BffOptions>(isDev);
        CookieAuth = config.CreateFromConfig<CookieAuthOptions>(isDev);
        Rpt = config.CreateFromConfig<RptOptions>(isDev);
    }

    public bool IsDev { get; }

    // /// <summary>
    // /// Authorization mode for the application
    // /// </summary>
    // public AuthorizationMode AuthorizationMode { get; set; }
    //
    // /// <summary>
    // /// Whether to enable antiforgery protection
    // /// </summary>
    // public bool EnableAntiforgery { get; set; } = true;
    
    public BffAntiforgeryOptions AntiForgery { get; set; }
    public Action<BffAntiforgeryOptions>? ConfigureAntiForgery { get; set; }

    public BffOptions Bff { get; set; }
    public Action<BffOptions>? ConfigureBff { get; set; }

    public CookieAuthOptions CookieAuth { get; set; }
    public Action<CookieAuthOptions>? ConfigureCookieAuth { get; set; }

    public RptOptions Rpt { get; set; }
    public Action<RptOptions>? ConfigureRpt { get; set; }

    // /// <summary>
    // /// Whether to enable reverse proxy functionality
    // /// </summary>
    // public bool EnableReverseProxy { get; set; } = true;

    // /// <summary>
    // /// Whether to only enable reverse proxy in development
    // /// </summary>
    // public bool OnlyReverseProxyInDevelopment { get; set; } = true;





    public void Configure(IServiceCollection services)
    {
        // Core configuration
        ConfigureCore(services);
        
        AntiForgery.Configure(services, ConfigureAntiForgery);
        Bff.Configure(services, ConfigureBff);
        CookieAuth.Configure(services, ConfigureCookieAuth);
        Rpt.Configure(services, ConfigureRpt);
    }
}