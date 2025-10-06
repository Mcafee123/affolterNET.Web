using System.Text.Json;
using System.Text.Json.Serialization;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Core.Options;

public abstract class CoreAppOptions
{
    protected CoreAppOptions(AppSettings appSettings, IConfiguration config)
    {
        AuthProvider = config.CreateFromConfig<AuthProviderOptions>(appSettings);
        Oidc = config.CreateFromConfig<OidcOptions>(appSettings);
        PermissionCache = config.CreateFromConfig<PermissionCacheOptions>(appSettings);
        SecurityHeaders = config.CreateFromConfig<SecurityHeadersOptions>(appSettings);
        Swagger = config.CreateFromConfig<SwaggerOptions>(appSettings);
        IsDev = appSettings.IsDev;
    }
    
    public bool IsDev { get; }
    
    /// <summary>
    /// Whether to enable security headers middleware
    /// </summary>
    public bool EnableSecurityHeaders { get; set; } = true;

    public AuthProviderOptions AuthProvider { get; set; }
    public Action<AuthProviderOptions>? ConfigureAuthProvider { get; set; }

    public OidcOptions Oidc { get; set; }
    public Action<OidcOptions>? ConfigureOidc { get; set; }

    public PermissionCacheOptions PermissionCache { get; set; }
    public Action<PermissionCacheOptions>? ConfigurePermissionCache { get; set; }

    public SecurityHeadersOptions SecurityHeaders { get; set; }
    public Action<SecurityHeadersOptions>? ConfigureSecurityHeaders { get; set; }

    public SwaggerOptions Swagger { get; set; }
    public Action<SwaggerOptions>? ConfigureSwagger { get; set; }

    protected void RunCoreActions(ConfigureActions? actions = null)
    {
        actions ??= new ConfigureActions(); // create if null
        actions.Add(ConfigureAuthProvider);
        actions.Add(ConfigureOidc);
        actions.Add(ConfigurePermissionCache);
        actions.Add(ConfigureSecurityHeaders);
        actions.Add(ConfigureSwagger);
        
        AuthProvider.RunActions(actions);
        Oidc.RunActions(actions);
        PermissionCache.RunActions(actions);
        SecurityHeaders.RunActions(actions);
        Swagger.RunActions(actions);
    }

    protected void ConfigureCoreDi(IServiceCollection services)
    {
        AuthProvider.ConfigureDi(services);
        Oidc.ConfigureDi(services);
        PermissionCache.ConfigureDi(services);
        SecurityHeaders.ConfigureDi(services);
        Swagger.ConfigureDi(services);
    }

    protected abstract Dictionary<string, object> GetConfigs();
    
    /// <summary>
    /// Serializes the BffAppOptions to JSON string for logging purposes
    /// </summary>
    /// <returns>JSON representation of the configuration</returns>
    public string ToJson()
    {
        var dict = GetConfigs();
        
        // add base properties to configuration dictionary
        AuthProvider.AddToConfigurationDict(dict);
        Oidc.AddToConfigurationDict(dict);
        PermissionCache.AddToConfigurationDict(dict);
        SecurityHeaders.AddToConfigurationDict(dict);
        Swagger.AddToConfigurationDict(dict);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Serialize(dict, options);
    }
}