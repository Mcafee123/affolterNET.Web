using System.Reflection;
using affolterNET.Web.Bff.Configuration;
using affolterNET.Web.Bff.Middleware;
using affolterNET.Web.Bff.Options;
using affolterNET.Web.Bff.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using affolterNET.Web.Core.Extensions;
using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Services;
using Microsoft.Extensions.Logging;

namespace affolterNET.Web.Bff.Extensions;

public static class ServiceCollectionExtensions
{
    private static ILogger? _logger;

    /// <summary>
    /// Adds complete BFF authentication with all required services and middleware
    /// This is the single public entry point for BFF authentication
    /// </summary>
    public static BffAppOptions AddBffServices(this IServiceCollection services, AppSettings appSettings, IConfiguration configuration,
        Action<BffAppOptions>? configureOptions = null)
    {
        _logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>()
            .CreateLogger(Assembly.GetEntryAssembly()?.GetName().Name!);

        // 1. Create BffAppOptions instance with constructor defaults
        var bffOptions = new BffAppOptions(appSettings, configuration);
        configureOptions?.Invoke(bffOptions);
        bffOptions.Configure(services);
        
        // Add core authentication services
        services.AddCoreServices()
            .AddKeycloakIntegration(bffOptions)
            .AddRptServices()
            .AddAuthorizationPolicies();
        
        // Swagger
        services.AddSwagger(bffOptions);
        
        // CORS
        services.AddCors(bffOptions.Cors);
        
        // Add BFF-specific authentication setup
        services.AddBffAuthenticationInternal(bffOptions);
        
        // Add BFF supporting services
        services.AddAntiforgeryServicesInternal(bffOptions.AntiForgery);
        services.AddReverseProxyInternal(configuration);
        return bffOptions;
    }

    /// <summary>
    /// Adds BFF authentication configuration (cookies, OIDC)
    /// </summary>
    private static IServiceCollection AddBffAuthenticationInternal(this IServiceCollection services, BffAppOptions bffOptions)
    {
        // Add authentication
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = bffOptions.CookieAuth.Name;
                options.Cookie.HttpOnly = bffOptions.CookieAuth.HttpOnly;
                options.Cookie.SecurePolicy = bffOptions.CookieAuth.Secure
                    ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always
                    : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = bffOptions.CookieAuth.SameSite switch
                {
                    "Strict" => Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                    "Lax" => Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    "None" => Microsoft.AspNetCore.Http.SameSiteMode.None,
                    _ => Microsoft.AspNetCore.Http.SameSiteMode.Strict
                };
                options.ExpireTimeSpan = bffOptions.CookieAuth.ExpireTimeSpan;
                options.SlidingExpiration = bffOptions.CookieAuth.SlidingExpiration;
                options.LoginPath = "/bff/account/login";
                options.LogoutPath = "/bff/account/logout";
                options.AccessDeniedPath = "/bff/access-denied";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = bffOptions.AuthProvider.Authority;
                options.ClientId = bffOptions.AuthProvider.ClientId;
                options.ClientSecret = bffOptions.AuthProvider.ClientSecret;
                options.ResponseType = bffOptions.Oidc.ResponseType;
                options.SaveTokens = bffOptions.Oidc.SaveTokens;
                options.UsePkce = bffOptions.Oidc.UsePkce;

                options.Scope.Clear();
                foreach (var scope in bffOptions.Oidc.GetScopes().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    options.Scope.Add(scope);
                }

                options.CallbackPath = bffOptions.BffAuth.RedirectUri;
                options.SignedOutCallbackPath = bffOptions.BffAuth.PostLogoutRedirectUri;

                // Map claims
                options.MapInboundClaims = false;
                options.GetClaimsFromUserInfoEndpoint = true;
            });

        // Register BFF-specific services (only services from this library)
        services.AddSingleton<TokenRefreshService>();
        services.AddScoped<IClaimsEnrichmentService, BffClaimsEnrichmentService>();
        services.AddScoped<IBffSessionService, BffSessionService>();
        services.AddHttpClient<IBffApiClient, BffApiClient>();

        return services;
    }

    /// <summary>
    /// Adds reverse proxy with authentication token forwarding
    /// </summary>
    private static IServiceCollection AddReverseProxyInternal(this IServiceCollection services,
        IConfiguration configuration, string sectionKey = "affolterNET:ReverseProxy")
    {
        // Check if reverse proxy configuration exists and if no, add a minimal config
        var reverseProxySection = configuration.GetSection(sectionKey);
        var hasReverseProxyConfig = reverseProxySection.Exists() && reverseProxySection.GetChildren().Any();
        if (!hasReverseProxyConfig)
        {
            // Add minimal default configuration
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ReverseProxy:Routes"] = "",
                ["ReverseProxy:Clusters"] = ""
            }!);
            var defaultConfig = configBuilder.Build();
            reverseProxySection = defaultConfig.GetSection(sectionKey);

            // Log a warning that reverse proxy configuration is missing
            _logger?.LogWarning("Using default empty YARP configuration");
        }

        services.AddReverseProxy()
            .LoadFromConfig(reverseProxySection)
            .AddTransforms<AuthTransform>();

        return services;
    }

    /// <summary>
    /// Adds antiforgery services configured for SPA scenarios with client-accessible tokens
    /// </summary>
    private static IServiceCollection AddAntiforgeryServicesInternal(this IServiceCollection services, BffAntiforgeryOptions bffAntiforgeryOptions)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = bffAntiforgeryOptions.ClientCookieName;
            options.Cookie.Name = bffAntiforgeryOptions.ServerCookieName;
            options.Cookie.SameSite = bffAntiforgeryOptions.SameSiteMode;
            options.Cookie.SecurePolicy = bffAntiforgeryOptions.RequireSecure
                ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always
                : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.Cookie.Path = bffAntiforgeryOptions.CookiePath;
        });

        return services;
    }
}