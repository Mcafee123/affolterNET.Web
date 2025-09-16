using System.Text.Json;
using System.Text.Json.Serialization;
using affolterNET.Web.Bff.Configuration;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Options;
using affolterNET.Web.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Bff.Options;

/// <summary>
/// Configuration options for BFF application pipeline
/// </summary>
public class BffAppOptions : CoreAppOptions
{
    public BffAppOptions(AppSettings appSettings, IConfiguration config): base(appSettings, config)
    {
        IsDev = appSettings.IsDev;
        AntiForgery = config.CreateFromConfig<BffAntiforgeryOptions>(appSettings);
        Bff = config.CreateFromConfig<BffOptions>(appSettings);
        CookieAuth = config.CreateFromConfig<CookieAuthOptions>(appSettings);
        Rpt = config.CreateFromConfig<RptOptions>(appSettings);
        BffAuth = config.CreateFromConfig<BffAuthOptions>(appSettings);
    }
    
    public bool IsDev { get; }

    public BffAuthOptions BffAuth { get; set; }
    public Action<BffAuthOptions>? ConfigureAuth { get; set; }
    
    public BffAntiforgeryOptions AntiForgery { get; set; }
    public Action<BffAntiforgeryOptions>? ConfigureAntiForgery { get; set; }

    public BffOptions Bff { get; set; }
    public Action<BffOptions>? ConfigureBff { get; set; }

    public CookieAuthOptions CookieAuth { get; set; }
    public Action<CookieAuthOptions>? ConfigureCookieAuth { get; set; }

    public RptOptions Rpt { get; set; }
    public Action<RptOptions>? ConfigureRpt { get; set; }

    public void Configure(IServiceCollection services)
    {
        var actions = new ConfigureActions();
        actions.Add(ConfigureAntiForgery);
        actions.Add(ConfigureAuth);
        actions.Add(ConfigureBff);
        actions.Add(ConfigureCookieAuth);
        actions.Add(ConfigureRpt);
        
        AntiForgery.Configure(services, actions);
        BffAuth.Configure(services, actions);
        Bff.Configure(services, actions);
        CookieAuth.Configure(services, actions);
        Rpt.Configure(services, actions);
        
        // Move config values to base types
        var baseActions = new ConfigureActions();
        Action<SecurityHeadersOptions> configureDevUrl = sho =>
        {
            sho.UiDevServerUrl = Bff.UiDevServerUrl;
        };
        baseActions.Add(configureDevUrl);

        // Core configuration
        ConfigureCore(services, baseActions);
    }

    /// <summary>
    /// Serializes the BffAppOptions to JSON string for logging purposes
    /// </summary>
    /// <returns>JSON representation of the configuration</returns>
    public string ToJson()
    {
        var result = new Dictionary<string, object>();
        Bff.AddToConfigurationDict(result);
        CookieAuth.AddToConfigurationDict(result);
        Oidc.AddToConfigurationDict(result);
        AuthProvider.AddToConfigurationDict(result);
        BffAuth.AddToConfigurationDict(result);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Serialize(result, options);
    }
}