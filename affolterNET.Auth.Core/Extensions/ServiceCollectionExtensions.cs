using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using affolterNET.Auth.Core.Configuration;
using affolterNET.Auth.Core.Authorization;
using affolterNET.Auth.Core.Middleware;
using affolterNET.Auth.Core.Services;
using NETCore.Keycloak.Client.HttpClients.Abstraction;
using NETCore.Keycloak.Client.HttpClients.Implementation;

namespace affolterNET.Auth.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds complete authentication services with all middleware, token refresh, and authorization policies
    /// Used by BFF authentication
    /// </summary>
    public static IServiceCollection AddCompleteAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register unified Auth configuration
        services.Configure<AuthConfiguration>(configuration.GetSection(AuthConfiguration.SectionName));

        // Register Keycloak client
        var authConfig = AuthConfiguration.Bind(configuration);
        services.AddSingleton<IKeycloakClient>(_ => new KeycloakClient(authConfig.AuthorityBase));

        // Register RPT and token services
        services.AddScoped<TokenHelper>();
        services.AddScoped<RptTokenService>();
        services.AddSingleton<RptCacheService>();
        services.AddScoped<AuthClaimsService>();
        services.AddScoped<IPermissionService, PermissionService>();

        // Add memory cache if not already added
        services.AddMemoryCache();

        // Add HTTP context accessor if not already added
        services.AddHttpContextAccessor();

        // Add authorization and custom policy provider/handler
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Adds security headers configuration and middleware services
    /// Used by BFF authentication
    /// </summary>
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        // Register AuthConfiguration to access SecurityHeaders settings
        var authConfig = AuthConfiguration.Bind(configuration);
        services.Configure<AuthConfiguration>(config => 
        {
            config.SecurityHeaders = authConfig.SecurityHeaders;
        });
        
        return services;
    }
}
