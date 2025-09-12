using Microsoft.AspNetCore.Builder;
using affolterNET.Auth.Bff.Middleware;
using affolterNET.Auth.Core.Middleware;

namespace affolterNET.Auth.Bff.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds BFF authentication middleware pipeline with proper ordering
    /// </summary>
    public static IApplicationBuilder UseBffAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseMiddleware<RefreshTokenMiddleware>();
        app.UseMiddleware<RptMiddleware>(); // Custom claims enrichment
        app.UseAuthorization();
        
        return app;
    }
    
    /// <summary>
    /// Adds BFF authentication middleware pipeline with antiforgery token generation
    /// </summary>
    public static IApplicationBuilder UseBffAuthenticationWithAntiforgery(this IApplicationBuilder app)
    {
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseMiddleware<RefreshTokenMiddleware>();
        app.UseMiddleware<RptMiddleware>();
        app.UseAuthorization();
        app.UseMiddleware<AntiforgeryTokenMiddleware>();
        
        return app;
    }
    
    /// <summary>
    /// Adds only the antiforgery token middleware for existing authentication pipelines
    /// </summary>
    public static IApplicationBuilder UseAntiforgeryTokenMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AntiforgeryTokenMiddleware>();
    }
    
    /// <summary>
    /// Adds BFF authentication middleware pipeline with security headers
    /// </summary>
    public static IApplicationBuilder UseBffAuthenticationWithSecurityHeaders(this IApplicationBuilder app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseMiddleware<RefreshTokenMiddleware>();
        app.UseMiddleware<RptMiddleware>();
        app.UseAuthorization();
        
        return app;
    }
    
    /// <summary>
    /// Adds complete BFF authentication middleware pipeline with security headers and antiforgery
    /// </summary>
    public static IApplicationBuilder UseCompleteBffAuthentication(this IApplicationBuilder app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseMiddleware<RefreshTokenMiddleware>();
        app.UseMiddleware<RptMiddleware>();
        app.UseAuthorization();
        app.UseMiddleware<AntiforgeryTokenMiddleware>();
        
        return app;
    }
    
    /// <summary>
    /// Adds only the security headers middleware for existing authentication pipelines
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}