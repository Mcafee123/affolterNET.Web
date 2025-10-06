using affolterNET.Web.Api.Options;
using Microsoft.AspNetCore.Builder;
using affolterNET.Web.Core.Middleware;
using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder ConfigureApiApp(this IApplicationBuilder app,
        ApiAppOptions apiOptions)
    {
        // 1. SECURITY HEADERS (Always second - protects ALL responses)
        if (apiOptions.EnableSecurityHeaders)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
        }
        
        // 2. API DOCUMENTATION (Swagger/OpenAPI) - After security, before routing
        if (apiOptions.Swagger.EnableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json",
                    $"{apiOptions.Swagger.Title} {apiOptions.Swagger.Version}");
            });
        }
        
        // 3. ROUTING (Required before auth)
        app.UseRouting();
        
        // 4. AUTHENTICATION & AUTHORIZATION PIPELINE
        if (apiOptions.ApiJwtBearer.AuthMode != AuthenticationMode.None)
        {
            // not implemented yet
        }
        
        // 5. ENDPOINT MAPPING
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }

    /// <summary>
    /// Adds API authentication middleware pipeline with proper ordering
    /// </summary>
    private static IApplicationBuilder UseApiAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseMiddleware<RptMiddleware>();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Adds API authentication middleware pipeline with security headers
    /// </summary>
    private static IApplicationBuilder UseApiAuthenticationWithSecurityHeaders(this IApplicationBuilder app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<RptMiddleware>();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Adds API authentication middleware pipeline with security headers
    /// </summary>
    private static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}