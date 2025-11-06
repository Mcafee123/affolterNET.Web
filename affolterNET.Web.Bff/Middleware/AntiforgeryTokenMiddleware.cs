using affolterNET.Web.Bff.Configuration;
using affolterNET.Web.Core.Configuration;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace affolterNET.Web.Bff.Middleware;

/// <summary>
/// Middleware that generates and sets antiforgery tokens for SPA applications.
/// Sets a client-accessible cookie containing the antiforgery token for JavaScript frameworks.
/// </summary>
public class AntiforgeryTokenMiddleware(
    RequestDelegate next,
    IAntiforgery antiforgery,
    IOptionsMonitor<BffAntiforgeryOptions> antiForgeryOptions,
    IOptions<CloudOptions> cloudOptions)
{
    public async Task Invoke(HttpContext context)
    {
        var options = antiForgeryOptions.CurrentValue;
        var requestPath = context.Request.Path;

        // Skip antiforgery token generation for health check endpoints
        // Health checks are called by Azure Container Apps over HTTP (internal network)
        // and don't require CSRF protection
        if (cloudOptions.Value.MapHealthChecks)
        {
            if (requestPath.StartsWithSegments("/health"))
            {
                await next(context);
                return;
            }
        }

        // Generate antiforgery token for GET requests to root or when explicitly requested
        if (requestPath == "/" || HttpMethods.IsGet(context.Request.Method))
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            var requestToken = tokens.RequestToken;

            if (!string.IsNullOrEmpty(requestToken))
            {
                context.Response.Cookies.Append(
                    options.ClientCookieName,
                    requestToken,
                    new CookieOptions
                    {
                        HttpOnly = false, // Must be accessible to JavaScript
                        IsEssential = true,
                        Secure = context.Request.IsHttps || options.RequireSecure,
                        SameSite = options.SameSiteMode,
                        Path = options.CookiePath
                    });
            }
        }

        await next(context);
    }
}