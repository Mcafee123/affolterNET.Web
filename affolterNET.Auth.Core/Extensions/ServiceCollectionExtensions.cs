using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using affolterNET.Auth.Core.Configuration;
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
        
        // Add memory cache if not already added
        services.AddMemoryCache();
        
        // Add HTTP context accessor if not already added
        services.AddHttpContextAccessor();
        
        return services;
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
}