using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using affolterNET.Auth.Core.Extensions;
using affolterNET.Auth.Core.Services;
using affolterNET.Auth.Bff.Configuration;
using affolterNET.Auth.Bff.Services;
using affolterNET.Auth.Bff.Middleware;

namespace affolterNET.Auth.Bff.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds BFF authentication with core services
    /// </summary>
    public static IServiceCollection AddBffAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Add core authentication services including token refresh and authorization policies
        services.AddCompleteAuthServices(configuration);
        
        // Configure BFF-specific options
        services.Configure<BffAuthOptions>(configuration.GetSection(BffAuthOptions.SectionName));
        
        var bffAuthOptions = configuration.GetSection(BffAuthOptions.SectionName).Get<BffAuthOptions>();
        if (bffAuthOptions == null)
        {
            throw new InvalidOperationException($"Configuration section '{BffAuthOptions.SectionName}' is required");
        }

        // Add authentication
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "oidc";
            options.DefaultSignOutScheme = "oidc";
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = bffAuthOptions.Cookie.Name;
            options.Cookie.HttpOnly = bffAuthOptions.Cookie.HttpOnly;
            options.Cookie.SecurePolicy = bffAuthOptions.Cookie.Secure ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = bffAuthOptions.Cookie.SameSite switch
            {
                "Strict" => Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                "Lax" => Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                "None" => Microsoft.AspNetCore.Http.SameSiteMode.None,
                _ => Microsoft.AspNetCore.Http.SameSiteMode.Strict
            };
            options.ExpireTimeSpan = bffAuthOptions.Cookie.ExpireTimeSpan;
            options.SlidingExpiration = bffAuthOptions.Cookie.SlidingExpiration;
            options.LoginPath = "/bff/login";
            options.LogoutPath = "/bff/logout";
            options.AccessDeniedPath = "/bff/access-denied";
        })
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = bffAuthOptions.Authority;
            options.ClientId = bffAuthOptions.ClientId;
            options.ClientSecret = bffAuthOptions.ClientSecret;
            options.ResponseType = bffAuthOptions.Oidc.ResponseType;
            options.SaveTokens = bffAuthOptions.Oidc.SaveTokens;
            options.UsePkce = bffAuthOptions.Oidc.UsePkce;
            
            options.Scope.Clear();
            foreach (var scope in bffAuthOptions.GetScopes().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                options.Scope.Add(scope);
            }

            options.CallbackPath = bffAuthOptions.RedirectUri;
            options.SignedOutCallbackPath = bffAuthOptions.PostLogoutRedirectUri;
            
            // Map claims
            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = true;
        });

        // Register BFF-specific services
        services.AddSingleton<TokenRefreshService>();
        services.AddScoped<IClaimsEnrichmentService, BffClaimsEnrichmentService>();
        services.AddScoped<IBffSessionService, BffSessionService>();
        services.AddHttpClient<IBffApiClient, BffApiClient>();
        
        return services;
    }

    /// <summary>
    /// Adds reverse proxy with authentication token forwarding
    /// </summary>
    public static IServiceCollection AddReverseProxyWithAuthTransform(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms<AuthTransform>();
        
        return services;
    }

    /// <summary>
    /// Adds antiforgery services configured for SPA scenarios with client-accessible tokens
    /// </summary>
    public static IServiceCollection AddAntiforgeryServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authConfig = affolterNET.Auth.Core.Configuration.AuthConfiguration.Bind(configuration);
        
        services.AddAntiforgery(options =>
        {
            options.HeaderName = authConfig.Antiforgery.ClientCookieName;
            options.Cookie.Name = authConfig.Antiforgery.ServerCookieName;
            options.Cookie.SameSite = authConfig.Antiforgery.SameSiteMode;
            options.Cookie.SecurePolicy = authConfig.Antiforgery.RequireSecure 
                ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always 
                : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.Cookie.Path = authConfig.Antiforgery.CookiePath;
        });
        
        return services;
    }

    /// <summary>
    /// Adds complete BFF authentication with security headers, antiforgery, and reverse proxy
    /// This is the main method used by DataFlow.Web
    /// </summary>
    public static IServiceCollection AddCompleteBffAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddBffAuthentication(configuration)
                      .AddAntiforgeryServices(configuration)
                      .AddSecurityHeaders(configuration)
                      .AddReverseProxyWithAuthTransform(configuration);
    }
}