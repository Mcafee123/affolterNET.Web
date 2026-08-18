using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace affolterNET.Web.Core.Middleware;

/// <summary>
/// Requires a signed-in user for the Swagger UI and document.
///
/// The pipeline serves Swagger BEFORE routing and authentication (so the UI works without
/// endpoint mapping), which means the regular auth middleware never sees /swagger requests.
/// This middleware sits directly in front of the Swagger middleware and authenticates
/// explicitly: the scheme handler is invoked directly, no UseAuthentication needed. An
/// unauthenticated request is challenged - a cookie/OIDC app redirects to the login and
/// returns to /swagger afterwards, a JWT API answers 401 with WWW-Authenticate.
/// </summary>
public class SwaggerAuthMiddleware(RequestDelegate next, ILogger<SwaggerAuthMiddleware> logger)
{
    private const string SwaggerPathPrefix = "/swagger";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(SwaggerPathPrefix))
        {
            var result = await context.AuthenticateAsync();
            if (!result.Succeeded)
            {
                logger.LogDebug("Unauthenticated request to {Path} - challenging", context.Request.Path);
                await context.ChallengeAsync();
                return;
            }
            context.User = result.Principal;
        }
        await next(context);
    }
}
