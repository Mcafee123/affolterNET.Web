using affolterNET.Web.Api.Options;
using affolterNET.Web.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using affolterNET.Web.Core.Middleware;
using affolterNET.Web.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        
        // 4. CUSTOM MIDDLEWARE (After routing)
        apiOptions.ConfigureAfterRoutingCustomMiddleware?.Invoke(app);
        
        // 5. CORS (Must be after UseRouting and before UseAuthentication)
        if (apiOptions.Cors.Enabled)
        {
            // Validate CORS configuration at startup
            apiOptions.Cors.Validate(apiOptions.IsDev);
            app.UseCors();
        }
        
        // 6. AUTHENTICATION & AUTHORIZATION PIPELINE
        if (apiOptions.ApiJwtBearer.AuthMode != AuthenticationMode.None)
        {
            app.UseAuthentication();
            app.UseMiddleware<RptMiddleware>();
            app.UseAuthorization();
        }
        
        // 7. CUSTOM MIDDLEWARE (Before endpoint mapping)
        apiOptions.ConfigureBeforeEndpointsCustomMiddleware?.Invoke(app);
        
        // 8. ENDPOINT MAPPING
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }
}