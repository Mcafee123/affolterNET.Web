using affolterNET.Web.Core.Configuration;
using affolterNET.Web.Core.Extensions;
using affolterNET.Web.Core.Models;
using affolterNET.Web.Mcp.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Authentication;

namespace affolterNET.Web.Mcp.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Turns the application into an OAuth 2.1 resource server for its MCP endpoint:
    /// a JWT Bearer scheme that validates Keycloak access tokens, and the MCP
    /// challenge scheme that answers unauthenticated requests with the
    /// <c>WWW-Authenticate</c> header and serves the protected resource metadata
    /// document clients discover the authorization server from.
    /// </summary>
    /// <remarks>
    /// Registers its own scheme names, so it composes with a cookie-based BFF or a
    /// JWT API in the same host without touching their default schemes. The MCP
    /// server itself (<c>AddMcpServer().WithHttpTransport().WithTools&lt;T&gt;()</c>)
    /// stays with the application — this only adds the authentication in front of it.
    /// </remarks>
    public static McpAuthOptions AddMcpAuthentication(
        this IServiceCollection services,
        AppSettings appSettings,
        IConfiguration configuration,
        Action<McpAuthOptions>? configureOptions = null)
    {
        var options = configuration.CreateFromConfig<McpAuthOptions>(appSettings);
        configureOptions?.Invoke(options);

        // An app normally talks to one Keycloak: inherit it instead of repeating it.
        var provider = configuration.CreateFromConfig<AuthProviderOptions>(appSettings);
        if (string.IsNullOrWhiteSpace(options.AuthorityBase))
        {
            options.AuthorityBase = provider.AuthorityBase;
        }

        if (string.IsNullOrWhiteSpace(options.Realm))
        {
            options.Realm = provider.Realm;
        }

        options.Validate();
        services.AddSingleton(options);

        if (!options.Enabled)
        {
            return options;
        }

        services.AddAuthentication()
            .AddJwtBearer(McpAuthDefaults.BearerScheme, jwt =>
            {
                jwt.Authority = options.Authority;
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Authority,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = options.ClockSkew,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                };
            })
            .AddMcp(McpAuthDefaults.AuthenticationScheme, McpAuthDefaults.DisplayName, mcp =>
            {
                mcp.ForwardAuthenticate = McpAuthDefaults.BearerScheme;
                mcp.ResourceMetadata = BuildResourceMetadata(options);
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(McpAuthDefaults.PolicyName, policy =>
            {
                policy.AddAuthenticationSchemes(McpAuthDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx => ScopeClaims.Satisfies(ctx.User, options.RequiredScopes));
            });

        return options;
    }

    /// <summary>
    /// The RFC 9728 document an MCP client fetches to learn which authorization
    /// server issues tokens for this endpoint.
    /// </summary>
    internal static ProtectedResourceMetadata BuildResourceMetadata(McpAuthOptions options)
    {
        var metadata = new ProtectedResourceMetadata
        {
            // Left null on purpose when unset: the handler then derives the canonical
            // resource URI from the request, which stays correct behind a proxy.
            Resource = string.IsNullOrWhiteSpace(options.Resource) ? null : options.Resource,
            AuthorizationServers = { options.Authority },
            ResourceName = options.ResourceName,
            ResourceDocumentation = options.ResourceDocumentation,
        };

        foreach (var scope in options.AdvertisedScopes)
        {
            metadata.ScopesSupported.Add(scope);
        }

        return metadata;
    }
}
