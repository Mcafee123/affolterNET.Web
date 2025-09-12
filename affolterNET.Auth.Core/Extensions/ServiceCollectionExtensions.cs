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
    public static IServiceCollection AddAuthCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthConfiguration>(configuration.GetSection(AuthConfiguration.SectionName));
        
        // Core services will be registered by specific implementations (API/BFF libraries)
        
        return services;
    }

    /// <summary>
    /// Adds RPT (Request Party Token) services for Keycloak authorization
    /// </summary>
    public static IServiceCollection AddRptServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register unified Auth configuration
        services.Configure<AuthConfiguration>(configuration.GetSection(AuthConfiguration.SectionName));
        
        // Register Keycloak client
        var authConfig = AuthConfiguration.Bind(configuration);
        services.AddSingleton<IKeycloakClient>(_ => new KeycloakClient(authConfig.AuthorityBase));
        
        // Register RPT services
        services.AddScoped<TokenHelper>();
        services.AddScoped<RptTokenService>();
        services.AddScoped<RptCacheService>();
        services.AddScoped<AuthClaimsService>();
        services.AddScoped<IPermissionService, PermissionService>();
        
        // Add memory cache if not already added
        services.AddMemoryCache();
        
        // Add HTTP context accessor if not already added
        services.AddHttpContextAccessor();
        
        return services;
    }

    /// <summary>
    /// Adds token refresh services for automatic token renewal
    /// </summary>
    public static IServiceCollection AddTokenRefreshServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register unified Auth configuration
        services.Configure<AuthConfiguration>(configuration.GetSection(AuthConfiguration.SectionName));
        
        // Register Keycloak client
        var authConfig = AuthConfiguration.Bind(configuration);
        services.AddSingleton<IKeycloakClient>(_ => new KeycloakClient(authConfig.AuthorityBase));
        
        // Register token refresh service
        services.AddScoped<TokenRefreshService>();
        
        // Add HTTP context accessor if not already added
        services.AddHttpContextAccessor();
        
        return services;
    }

    /// <summary>
    /// Adds authorization policy services for permission-based authorization
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization();
        
        // Register custom authorization policy provider and handler
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        
        return services;
    }

    /// <summary>
    /// Adds all authentication services including RPT and token refresh
    /// </summary>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register unified Auth configuration
        services.Configure<AuthConfiguration>(configuration.GetSection(AuthConfiguration.SectionName));
        
        // Register Keycloak client
        var authConfig = AuthConfiguration.Bind(configuration);
        services.AddSingleton<IKeycloakClient>(_ => new KeycloakClient(authConfig.AuthorityBase));
        
        // Register all services
        services.AddScoped<TokenHelper>();
        services.AddScoped<RptTokenService>();
        services.AddScoped<RptCacheService>();
        services.AddScoped<AuthClaimsService>();
        services.AddScoped<TokenRefreshService>();
        
        // Add memory cache if not already added
        services.AddMemoryCache();
        
        // Add HTTP context accessor if not already added
        services.AddHttpContextAccessor();
        
        return services;
    }

    /// <summary>
    /// Adds complete authentication services with all middleware, token refresh, and authorization policies
    /// </summary>
    public static IServiceCollection AddCompleteAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddAuthenticationServices(configuration)
                      .AddTokenRefreshServices(configuration)
                      .AddAuthorizationPolicies();
    }

    /// <summary>
    /// Adds security headers configuration and middleware services
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

    /// <summary>
    /// Adds authentication services with security headers
    /// </summary>
    public static IServiceCollection AddAuthServicesWithSecurityHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddAuthenticationServices(configuration)
                      .AddSecurityHeaders(configuration);
    }

    /// <summary>
    /// Adds complete authentication services with security headers, token refresh, and authorization policies
    /// </summary>
    public static IServiceCollection AddCompleteAuthServicesWithSecurityHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCompleteAuthServices(configuration)
                      .AddSecurityHeaders(configuration);
    }
}

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the RPT middleware to the pipeline for automatic claims enrichment
    /// </summary>
    public static IApplicationBuilder UseRptMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RptMiddleware>();
    }

    /// <summary>
    /// Adds the token refresh middleware to the pipeline for automatic token renewal
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="oidcScheme">The OIDC authentication scheme name (default: "OpenIdConnect")</param>
    public static IApplicationBuilder UseTokenRefreshMiddleware(this IApplicationBuilder app, string oidcScheme = "OpenIdConnect")
    {
        return app.UseMiddleware<RefreshTokenMiddleware>(oidcScheme);
    }

    /// <summary>
    /// Adds both RPT and token refresh middleware to the pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="oidcScheme">The OIDC authentication scheme name (default: "OpenIdConnect")</param>
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app, string oidcScheme = "OpenIdConnect")
    {
        return app.UseTokenRefreshMiddleware(oidcScheme)
                  .UseRptMiddleware();
    }
}