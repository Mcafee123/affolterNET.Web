using System.Text.Json;
using System.Text.Json.Serialization;
using affolterNET.Web.Bff.Configuration;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Options;
using affolterNET.Web.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Bff.Options;

/// <summary>
/// Configuration options for BFF application pipeline
/// </summary>
public class BffAppOptions : CoreAppOptions
{
    private readonly AppSettings _appSettings;

    public BffAppOptions(AppSettings appSettings, IConfiguration config) : base(appSettings, config)
    {
        _appSettings = appSettings;
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

        AntiForgery.RunActions(actions);
        BffAuth.RunActions(actions);
        Bff.RunActions(actions);
        CookieAuth.RunActions(actions);
        Rpt.RunActions(actions);
        RunCoreActions(actions);

        // Move config values to base types
        var baseActions = new ConfigureActions();
        Action<SecurityHeadersOptions> configureUrl = sho =>
        {
            sho.FrontendUrl = _appSettings.IsDev ? Bff.UiDevServerUrl : Bff.BackendUrl;
            sho.IdpHost = AuthProvider.AuthorityBase;
        };
        baseActions.Add(configureUrl);
        RunCoreActions(baseActions);

        // configure DI
        AntiForgery.ConfigureDi(services);
        BffAuth.ConfigureDi(services);
        Bff.ConfigureDi(services);
        CookieAuth.ConfigureDi(services);
        Rpt.ConfigureDi(services);
        
        // Core configuration
        ConfigureCoreDi(services);
    }

    /// <summary>
    /// Serializes the BffAppOptions to JSON string for logging purposes
    /// </summary>
    /// <returns>JSON representation of the configuration</returns>
    public string ToJson()
    {
        var configDict = new Dictionary<string, object>();
        Bff.AddToConfigurationDict(configDict);
        CookieAuth.AddToConfigurationDict(configDict);
        Oidc.AddToConfigurationDict(configDict);
        AuthProvider.AddToConfigurationDict(configDict);
        BffAuth.AddToConfigurationDict(configDict);

        // add base properties to configuration dictionary
        AddCoreToConfigurationDict(configDict);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Serialize(configDict, options);
    }
}