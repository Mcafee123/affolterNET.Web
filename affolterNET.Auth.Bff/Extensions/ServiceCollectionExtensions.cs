using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using affolterNET.Auth.Core.Extensions;
using affolterNET.Auth.Core.Services;
using affolterNET.Auth.Bff.Configuration;
using affolterNET.Auth.Bff.Services;
using affolterNET.Auth.Bff.Middleware;
using affolterNET.Auth.Core.Models;
using Microsoft.Extensions.Logging;

namespace affolterNET.Auth.Bff.Extensions;

public static class ServiceCollectionExtensions
{
    private static ILogger? _logger;

    /// <summary>
    /// Adds complete BFF authentication with all required services and middleware
    /// This is the single public entry point for BFF authentication
    /// </summary>
    public static IServiceCollection AddBffServices(this IServiceCollection services,
        IConfiguration configuration, Action<SwaggerOpt>? swaggerOptions = null)
    {
        _logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>()
            .CreateLogger("affolterNET.Auth.Bff");

        // Add core authentication services
        services.AddCoreServices()
            .AddKeycloakIntegration(configuration)
            .AddRptServices()
            .AddAuthorizationPolicies();
        
        // Swagger
        services.AddSwagger(swaggerOptions);
        
        // Add BFF-specific authentication setup
        services.AddBffAuthenticationInternal(configuration);
        
        // Add BFF supporting services
        services.AddAntiforgeryServicesInternal(configuration);
        services.AddSecurityHeaders(configuration);
        services.AddReverseProxyInternal(configuration);
        return services;
    }

    /// <summary>
    /// Adds BFF authentication configuration (cookies, OIDC)
    /// </summary>
    private static IServiceCollection AddBffAuthenticationInternal(this IServiceCollection services,
        IConfiguration configuration)
    {
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
                options.Cookie.SecurePolicy = bffAuthOptions.Cookie.Secure
                    ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always
                    : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
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
        IConfiguration configuration)
    {
        // Check if reverse proxy configuration exists and if no, add a minimal config
        var reverseProxySection = configuration.GetSection("ReverseProxy");
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
            reverseProxySection = defaultConfig.GetSection("ReverseProxy");

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
    private static IServiceCollection AddAntiforgeryServicesInternal(this IServiceCollection services,
        IConfiguration configuration)
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
}