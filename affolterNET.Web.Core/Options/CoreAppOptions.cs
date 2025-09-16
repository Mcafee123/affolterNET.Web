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
    }
    
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

    protected void ConfigureCore(IServiceCollection services, ConfigureActions? actions = null)
    {
        actions ??= new ConfigureActions(); // create if null
        actions.Add(ConfigureAuthProvider);
        actions.Add(ConfigureOidc);
        actions.Add(ConfigurePermissionCache);
        actions.Add(ConfigureSecurityHeaders);
        actions.Add(ConfigureSwagger);
        
        AuthProvider.Configure(services, actions);
        Oidc.Configure(services, actions);
        PermissionCache.Configure(services, actions);
        SecurityHeaders.Configure(services, actions);
        Swagger.Configure(services, actions);
    }
}