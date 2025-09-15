using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using affolterNET.Auth.Core.Configuration;
using affolterNET.Auth.Core.Authorization;
using affolterNET.Auth.Core.Models;
using affolterNET.Auth.Core.Services;
using NETCore.Keycloak.Client.HttpClients.Abstraction;
using NETCore.Keycloak.Client.HttpClients.Implementation;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace affolterNET.Auth.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core infrastructure services required for authentication (memory cache, HTTP context accessor)
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Add memory cache if not already added
        services.AddMemoryCache();

        // Add HTTP context accessor if not already added
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Adds Keycloak client integration and configuration
    /// </summary>
    public static IServiceCollection AddKeycloakIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register unified Auth configuration
        services.Configure<AuthConfiguration>(configuration.GetSection(AuthConfiguration.SectionName));

        // Register Keycloak client
        var authConfig = AuthConfiguration.Bind(configuration);
        services.AddSingleton<IKeycloakClient>(_ => new KeycloakClient(authConfig.AuthorityBase));

        return services;
    }

    /// <summary>
    /// Adds RPT (Request Party Token) services for permission management
    /// </summary>
    public static IServiceCollection AddRptServices(this IServiceCollection services)
    {
        // Register RPT and token services
        services.AddScoped<TokenHelper>();
        services.AddScoped<RptTokenService>();
        services.AddSingleton<RptCacheService>();
        services.AddScoped<AuthClaimsService>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }

    /// <summary>
    /// Adds authorization policies and custom authorization handlers
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        // Add authorization and custom policy provider/handler
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Adds security headers configuration and middleware services
    /// </summary>
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        // Register AuthConfiguration to access SecurityHeaders settings
        var authConfig = AuthConfiguration.Bind(configuration);
        services.Configure<AuthConfiguration>(config => { config.SecurityHeaders = authConfig.SecurityHeaders; });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services, Action<SwaggerOpt>? swaggerOptions = null)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var options = new SwaggerOpt();
            swaggerOptions?.Invoke(options);

            c.SwaggerDoc(options.Version, new() { Title = options.Title, Version = options.Version });

            // Add XML comments if available
            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
        return services;
    }
}