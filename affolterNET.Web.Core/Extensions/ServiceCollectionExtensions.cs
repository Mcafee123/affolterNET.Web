using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Authorization;
using affolterNET.Web.Core.Services;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.HttpClients.Abstraction;
using NETCore.Keycloak.Client.HttpClients.Implementation;

namespace affolterNET.Web.Core.Extensions;

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
        AuthProviderOptions authProviderConfig)
    {
        // Register Keycloak client
        services.AddSingleton<IKeycloakClient>(_ => new KeycloakClient(authProviderConfig.AuthorityBase));

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
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
    {
        // services.Configure<AuthConfiguration>(config => { config.SecurityHeaders = authConfig.SecurityHeaders; });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services, SwaggerOptions swaggerOptions)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(swaggerOptions.Version, new() { Title = swaggerOptions.Title, Version = swaggerOptions.Version });

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