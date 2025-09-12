using affolterNET.Auth.Core.Middleware;
using Microsoft.AspNetCore.Builder;

namespace affolterNET.Auth.Core.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the RPT middleware to the pipeline for automatic claims enrichment
    /// </summary>
    public static IApplicationBuilder UseRptMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RptMiddleware>();
    }
}