using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using affolterNET.Auth.Bff.Middleware;
using affolterNET.Auth.Bff.Models;
using affolterNET.Auth.Core.Middleware;

namespace affolterNET.Auth.Bff.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the complete BFF application pipeline with proper middleware order and flexibility
    /// This method ensures correct security practices and prevents middleware ordering mistakes
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <param name="configureOptions">Optional configuration for customizing behavior</param>
    public static IApplicationBuilder ConfigureBffApp(this IApplicationBuilder app, 
        bool isDevelopment, 
        Action<BffAppOptions>? configureOptions = null)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var options = new BffAppOptions();
        configureOptions?.Invoke(options);

        // 1. EXCEPTION HANDLING (Always first)
        if (isDevelopment)
        {
            app.UseDeveloperExceptionPage();
            // Note: WebAssembly debugging should be configured in the main application
        }
        else
        {
            app.UseExceptionHandler(options.ErrorPath);
        }

        // 2. SECURITY HEADERS (Always second - protects ALL responses)
        if (options.EnableSecurityHeaders)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        // 3. HTTPS REDIRECTION
        if (options.EnableHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        // 4. STATIC FILES
        if (options.EnableStaticFiles)
        {
            app.UseStaticFiles();
        }

        // 5. DEVELOPMENT TOOLS
        // 5. API DOCUMENTATION (Swagger/OpenAPI) - After security, before routing
        options.ConfigureApiDocumentation?.Invoke(app);

        // 6. ROUTING (Required before auth)
        app.UseRouting();

        // 7. ANTIFORGERY (Always enabled for CSRF protection, regardless of auth mode)
        if (options.EnableAntiforgery)
        {
            app.UseAntiforgery();
        }

        // 8. AUTHENTICATION & AUTHORIZATION PIPELINE
        if (options.AuthorizationMode != AuthorizationMode.None)
        {
            app.UseAuthentication();

            if (options.EnableTokenRefresh)
            {
                app.UseMiddleware<RefreshTokenMiddleware>();
            }

            if (options is { AuthorizationMode: AuthorizationMode.PermissionBased, EnableRptTokens: true })
            {
                app.UseMiddleware<RptMiddleware>();
            }

            // Custom authorization handling for API routes
            if (options.EnableNoUnauthorizedRedirect)
            {
                app.UseNoUnauthorizedRedirect(options.ApiRoutePrefix);
            }

            app.UseAuthorization();
        }

        // 9. ANTIFORGERY TOKEN MIDDLEWARE (After authorization)
        if (options.EnableAntiforgery)
        {
            app.UseMiddleware<AntiforgeryTokenMiddleware>();
        }

        // 10. CUSTOM MIDDLEWARE (After auth, before endpoint mapping)
        options.ConfigureCustomMiddleware?.Invoke(app);

        // 11. API 404 handling (Before endpoint mapping)
        if (options.EnableApiNotFound)
        {
            app.MapNotFound($"{options.ApiRoutePrefix}/{{**segment}}");
        }

        // 12. ENDPOINT MAPPING
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            
            // YARP Reverse Proxy
            if (ShouldEnableReverseProxy(configuration, isDevelopment, options))
            {
                endpoints.MapReverseProxy();
            }
            
            // Fallback to main page
            if (!string.IsNullOrEmpty(options.FallbackPage))
            {
                endpoints.MapFallbackToPage(options.FallbackPage);
            }
        });

        return app;
    }

    private static bool ShouldEnableReverseProxy(IConfiguration configuration, bool isDevelopment, BffAppOptions options)
    {
        if (!options.EnableReverseProxy)
            return false;

        if (isDevelopment && options.OnlyReverseProxyInDevelopment)
        {
            var uiDevServer = configuration.GetValue<string>("UiDevServerUrl");
            return !string.IsNullOrEmpty(uiDevServer);
        }

        return true;
    }
}