using affolterNET.Web.Bff.Configuration;
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
    IOptionsMonitor<BffAntiforgeryOptions> antiForgeryOptions)
{
    public async Task Invoke(HttpContext context)
    {
        var options = antiForgeryOptions.CurrentValue;
        var requestPath = context.Request.Path;
        
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