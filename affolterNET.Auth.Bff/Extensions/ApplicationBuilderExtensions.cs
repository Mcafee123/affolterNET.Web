using Microsoft.AspNetCore.Builder;
using affolterNET.Auth.Core.Middleware;

namespace affolterNET.Auth.Bff.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseBffAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseMiddleware<RptMiddleware>(); // Custom claims enrichment
        app.UseAuthorization();
        
        return app;
    }
}