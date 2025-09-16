using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Core.Options;

public abstract class CoreAppOptions
{
    protected CoreAppOptions(bool isDev, IConfiguration config)
    {
        AuthProvider = config.CreateFromConfig<AuthProviderOptions>(isDev);
        Oidc = config.CreateFromConfig<OidcOptions>(isDev);
        PermissionCache = config.CreateFromConfig<PermissionCacheOptions>(isDev);
        SecurityHeaders = config.CreateFromConfig<SecurityHeadersOptions>(isDev);
        Swagger = config.CreateFromConfig<SwaggerOptions>(isDev);
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

    protected void ConfigureCore(IServiceCollection services)
    {
        AuthProvider.Configure(services, ConfigureAuthProvider);
        Oidc.Configure(services, ConfigureOidc);
        PermissionCache.Configure(services, ConfigurePermissionCache);
        SecurityHeaders.Configure(services, ConfigureSecurityHeaders);
        Swagger.Configure(services, ConfigureSwagger);
    }
}