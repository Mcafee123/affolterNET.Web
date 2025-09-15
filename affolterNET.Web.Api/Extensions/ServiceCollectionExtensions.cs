using affolterNET.Web.Api.Configuration;
using affolterNET.Web.Api.Options;
using affolterNET.Web.Api.Services;
using affolterNET.Web.Core.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using affolterNET.Web.Core.Extensions;
using affolterNET.Web.Core.Services;
using Microsoft.Extensions.Logging;

namespace affolterNET.Web.Api.Extensions;

public static class ServiceCollectionExtensions
{
    private static ILogger? _logger;

    /// <summary>
    /// Adds complete API authentication with JWT Bearer, authorization policies, and security headers
    /// This is the single public entry point for API authentication
    /// </summary>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, bool isDev,
        IConfiguration configuration,
        Action<ApiAppOptions>? configureOptions = null)
    {
        _logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>()
            .CreateLogger("affolterNET.Auth.Bff");

        // 1. Create BffAppOptions instance with constructor defaults
        var apiOptions = new ApiAppOptions(isDev);
        configureOptions?.Invoke(apiOptions);
        apiOptions.Configure(services);

        // Add core authentication services
        services.AddCoreServices()
            .AddKeycloakIntegration(apiOptions.AuthProvider)
            .AddRptServices()
            .AddAuthorizationPolicies();

        // Add API-specific authentication setup
        services.AddApiAuthenticationInternal(apiOptions.AuthProvider, apiOptions.ApiJwtBearer);

        // Add security headers
        // services.AddSecurityHeaders(apiOptions.SecurityHeaders);

        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication and API-specific services
    /// </summary>
    private static IServiceCollection AddApiAuthenticationInternal(this IServiceCollection services,
        AuthProviderOptions authProviderOptions, ApiJwtBearerOptions jwtBearerOptions)
    {
        // Add JWT Bearer authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authProviderOptions.Authority;
                options.Audience = authProviderOptions.Audience;
                options.RequireHttpsMetadata = jwtBearerOptions.RequireHttpsMetadata;
                options.SaveToken = jwtBearerOptions.SaveToken;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtBearerOptions.ValidateIssuer,
                    ValidateAudience = jwtBearerOptions.ValidateAudience,
                    ValidateLifetime = jwtBearerOptions.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtBearerOptions.ValidateIssuerSigningKey,
                    ValidIssuers = jwtBearerOptions.ValidIssuers.Length > 0 ? jwtBearerOptions.ValidIssuers : null,
                    ValidAudiences =
                        jwtBearerOptions.ValidAudiences.Length > 0 ? jwtBearerOptions.ValidAudiences : null,
                    ClockSkew = jwtBearerOptions.ClockSkew
                };
            });

        // Register API-specific services (only services from this library)
        services.AddScoped<IClaimsEnrichmentService, ApiClaimsEnrichmentService>();

        return services;
    }
}