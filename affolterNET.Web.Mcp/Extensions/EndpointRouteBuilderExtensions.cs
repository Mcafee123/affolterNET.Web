using affolterNET.Web.Mcp.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Mcp.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the MCP Streamable-HTTP transport behind the OAuth policy registered by
    /// <c>AddMcpAuthentication</c>. Antiforgery is switched off for the endpoint:
    /// MCP clients are not browsers and carry no CSRF cookie.
    /// </summary>
    /// <returns>
    /// The endpoint builder, or <see langword="null"/> when the MCP endpoint is disabled by configuration.
    /// </returns>
    /// <remarks>
    /// Call after the BFF/API pipeline has been configured, so the SPA fallback does
    /// not swallow the route.
    /// </remarks>
    public static IEndpointConventionBuilder? MapMcpSecured(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<McpAuthOptions>();
        if (!options.Enabled)
        {
            return null;
        }

        return endpoints.MapMcp(options.Path)
            .RequireAuthorization(McpAuthDefaults.PolicyName)
            .DisableAntiforgery();
    }
}
