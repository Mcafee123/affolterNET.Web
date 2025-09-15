using affolterNET.Web.Bff.Configuration;
using Microsoft.AspNetCore.Builder;
using affolterNET.Web.Bff.Middleware;
using affolterNET.Web.Bff.Models;
using affolterNET.Web.Bff.Options;
using affolterNET.Web.Core.Middleware;

namespace affolterNET.Web.Bff.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the complete BFF application pipeline with proper middleware order and flexibility
    /// This method ensures correct security practices and prevents middleware ordering mistakes
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="bffOptions"></param>
    public static IApplicationBuilder ConfigureBffApp(this IApplicationBuilder app, BffAppOptions bffOptions)
    {
        // 1. EXCEPTION HANDLING (Always first)
        if (bffOptions.IsDev)
        {
            app.UseDeveloperExceptionPage();
            // Note: WebAssembly debugging should be configured in the main application
        }
        else
        {
            app.UseExceptionHandler(bffOptions.ErrorPath);
        }

        // 2. SECURITY HEADERS (Always second - protects ALL responses)
        if (bffOptions.EnableSecurityHeaders)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        // 3. HTTPS REDIRECTION
        if (bffOptions.EnableHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        // 4. STATIC FILES
        if (bffOptions.EnableStaticFiles)
        {
            app.UseStaticFiles();
        }

        // 5. DEVELOPMENT TOOLS
        // 5. API DOCUMENTATION (Swagger/OpenAPI) - After security, before routing
        bffOptions.ConfigureApiDocumentation?.Invoke(app);

        // 6. ROUTING (Required before auth)
        app.UseRouting();

        // 7. ANTIFORGERY (Always enabled for CSRF protection, regardless of auth mode)
        if (bffOptions.EnableAntiforgery)
        {
            app.UseAntiforgery();
        }

        // 8. AUTHENTICATION & AUTHORIZATION PIPELINE
        if (bffOptions.AuthorizationMode != AuthorizationMode.None)
        {
            app.UseAuthentication();

            if (bffOptions.EnableTokenRefresh)
            {
                app.UseMiddleware<RefreshTokenMiddleware>();
            }

            if (bffOptions is { AuthorizationMode: AuthorizationMode.PermissionBased, EnableRptTokens: true })
            {
                app.UseMiddleware<RptMiddleware>();
            }

            // Custom authorization handling for API routes
            if (bffOptions.EnableNoUnauthorizedRedirect)
            {
                app.UseMiddleware<NoUnauthorizedRedirectMiddleware>((object)bffOptions.ApiRoutePrefixes);
            }

            app.UseAuthorization();
        }

        // 9. ANTIFORGERY TOKEN MIDDLEWARE (After authorization)
        if (bffOptions.EnableAntiforgery)
        {
            app.UseMiddleware<AntiforgeryTokenMiddleware>();
        }

        // 10. CUSTOM MIDDLEWARE (After auth, before endpoint mapping)
        bffOptions.ConfigureCustomMiddleware?.Invoke(app);

        // 11. API 404 handling (Before endpoint mapping)
        if (bffOptions.EnableApiNotFound)
        {
            app.UseMiddleware<ApiNotFoundMiddleware>((object)bffOptions.ApiRoutePrefixes);
        }

        // 12. ENDPOINT MAPPING
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            
            // YARP Reverse Proxy
            endpoints.MapReverseProxy();
            
            // Fallback to main page
            if (!string.IsNullOrEmpty(bffOptions.FallbackPage))
            {
                endpoints.MapFallbackToPage(bffOptions.FallbackPage);
            }
        });

        return app;
    }
}