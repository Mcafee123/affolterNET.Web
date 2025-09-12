using Microsoft.AspNetCore.Builder;
using affolterNET.Auth.Bff.Middleware;
using affolterNET.Auth.Core.Middleware;

namespace affolterNET.Auth.Bff.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds complete BFF authentication middleware pipeline with security headers and antiforgery
    /// This is the main method used by DataFlow.Web
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
}