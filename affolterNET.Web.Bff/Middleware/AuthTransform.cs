using affolterNET.Web.Bff.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace affolterNET.Web.Bff.Middleware;

/// <summary>
/// YARP transform that adds authentication tokens to proxied requests
/// </summary>
public class AuthTransform(IOptionsMonitor<BffOptions> bffOptions, ILogger<AuthTransform> logger)
    : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // No validation needed
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        // No validation needed
    }

    public void Apply(TransformBuilderContext context)
    {
        var uiDevServer = bffOptions.CurrentValue.FrontendUrl;
        
        context.AddRequestTransform(async reqTransformContext =>
        {
            // Ignore dev server requests
            if (!string.IsNullOrWhiteSpace(uiDevServer) && 
                reqTransformContext.DestinationPrefix.StartsWith(uiDevServer))
            {
                logger.LogDebug("AuthTransform: Ignoring request to dev server: {url}", 
                    reqTransformContext.HttpContext.ContextForLogger());
                return;
            }

            // Get access token from the authenticated user
            var accessToken = await reqTransformContext.HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                // Add the access token as bearer token to the proxied request
                reqTransformContext.ProxyRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                logger.LogDebug("AuthTransform: Access token added to request: {url}", 
                    reqTransformContext.HttpContext.ContextForLogger());
                return;
            }

            logger.LogDebug("AuthTransform: No access token found to add to proxied-request: {url}", 
                reqTransformContext.HttpContext.ContextForLogger());
        });
    }
}

/// <summary>
/// Helper class for logging HTTP context information
/// </summary>
public class HttpContextLogger
{
    private readonly HttpContext _context;

    public HttpContextLogger(HttpContext context)
    {
        _context = context;
    }

    public override string ToString()
    {
        return _context.Request.GetDisplayUrl();
    }
}

/// <summary>
/// Extension methods for HTTP context logging
/// </summary>
public static class HttpContextExtensions
{
    public static HttpContextLogger ContextForLogger(this HttpContext context)
    {
        return new HttpContextLogger(context);
    }
}