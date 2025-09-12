using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using affolterNET.Auth.Core.Extensions;
using affolterNET.Auth.Core.Services;
using affolterNET.Auth.Api.Configuration;
using affolterNET.Auth.Api.Services;

namespace affolterNET.Auth.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds complete API authentication with JWT Bearer, authorization policies, and security headers
    /// This is the single public entry point for API authentication
    /// </summary>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Add core authentication services
        services.AddCoreServices()
                .AddKeycloakIntegration(configuration)
                .AddRptServices()
                .AddAuthorizationPolicies();
        
        // Add API-specific authentication setup
        services.AddApiAuthenticationInternal(configuration);
        
        // Add security headers
        services.AddSecurityHeaders(configuration);
        
        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication and API-specific services
    /// </summary>
    private static IServiceCollection AddApiAuthenticationInternal(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure API-specific options
        services.Configure<ApiAuthOptions>(configuration.GetSection(ApiAuthOptions.SectionName));
        
        var apiAuthOptions = configuration.GetSection(ApiAuthOptions.SectionName).Get<ApiAuthOptions>();
        if (apiAuthOptions == null)
        {
            throw new InvalidOperationException($"Configuration section '{ApiAuthOptions.SectionName}' is required");
        }

        // Add JWT Bearer authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = apiAuthOptions.Authority;
                options.Audience = apiAuthOptions.Audience;
                options.RequireHttpsMetadata = apiAuthOptions.RequireHttpsMetadata;
                options.SaveToken = apiAuthOptions.SaveToken;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = apiAuthOptions.Validation.ValidateIssuer,
                    ValidateAudience = apiAuthOptions.Validation.ValidateAudience,
                    ValidateLifetime = apiAuthOptions.Validation.ValidateLifetime,
                    ValidateIssuerSigningKey = apiAuthOptions.Validation.ValidateIssuerSigningKey,
                    ValidIssuers = apiAuthOptions.Validation.ValidIssuers.Length > 0 ? apiAuthOptions.Validation.ValidIssuers : null,
                    ValidAudiences = apiAuthOptions.Validation.ValidAudiences.Length > 0 ? apiAuthOptions.Validation.ValidAudiences : null,
                    ClockSkew = apiAuthOptions.ClockSkew
                };
            });

        // Register API-specific services (only services from this library)
        services.AddScoped<IClaimsEnrichmentService, ApiClaimsEnrichmentService>();
        
        return services;
    }
}