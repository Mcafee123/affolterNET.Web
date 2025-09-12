using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using affolterNET.Auth.Core.Extensions;
using affolterNET.Auth.Core.Services;
using affolterNET.Auth.Bff.Configuration;
using affolterNET.Auth.Bff.Services;

namespace affolterNET.Auth.Bff.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBffAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Add core authentication services including token refresh
        services.AddAuthenticationServices(configuration);
        
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
        services.AddScoped<IClaimsEnrichmentService, BffClaimsEnrichmentService>();
        services.AddScoped<IBffSessionService, BffSessionService>();
        services.AddHttpClient<IBffApiClient, BffApiClient>();
        
        return services;
    }

    /// <summary>
    /// Adds BFF authentication with full token refresh and RPT support
    /// </summary>
    public static IServiceCollection AddBffAuthenticationWithTokenRefresh(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddBffAuthentication(configuration);
    }
}