using Microsoft.AspNetCore.Builder;
using affolterNET.Auth.Core.Middleware;

namespace affolterNET.Auth.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds API authentication middleware pipeline with proper ordering
    /// </summary>
    public static IApplicationBuilder UseApiAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseMiddleware<RefreshTokenMiddleware>();
        app.UseMiddleware<RptMiddleware>();
        app.UseAuthorization();
        
        return app;
    }
    
    /// <summary>
    /// Adds API authentication middleware pipeline with security headers
    /// </summary>
    public static IApplicationBuilder UseApiAuthenticationWithSecurityHeaders(this IApplicationBuilder app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<RefreshTokenMiddleware>();
        app.UseMiddleware<RptMiddleware>();
        app.UseAuthorization();
        
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