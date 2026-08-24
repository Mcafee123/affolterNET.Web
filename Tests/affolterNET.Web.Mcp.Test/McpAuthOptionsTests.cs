using affolterNET.Web.Core.Models;
using affolterNET.Web.Mcp;
using affolterNET.Web.Mcp.Configuration;
using affolterNET.Web.Mcp.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace affolterNET.Web.Mcp.Test;

public class McpAuthOptionsTests
{
    private static IConfiguration Config(params (string Key, string Value)[] entries) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(entries.Select(e => new KeyValuePair<string, string?>(e.Key, e.Value)))
            .Build();

    [Fact]
    public void Authority_is_built_from_base_and_realm()
    {
        var options = new McpAuthOptions { AuthorityBase = "https://kc.example.com/", Realm = "demo" };

        Assert.Equal("https://kc.example.com/realms/demo", options.Authority);
    }

    [Fact]
    public void Disabled_by_default_so_an_app_must_opt_in()
    {
        var services = new ServiceCollection();

        var options = services.AddMcpAuthentication(new AppSettings(true, AuthenticationMode.Authenticate), Config());

        Assert.False(options.Enabled);
    }

    [Fact]
    public void Inherits_keycloak_settings_from_the_apps_auth_provider()
    {
        var services = new ServiceCollection();
        var config = Config(
            ("affolterNET:Web:Auth:Provider:AuthorityBase", "https://kc.example.com"),
            ("affolterNET:Web:Auth:Provider:Realm", "demo"),
            ("affolterNET:Web:Mcp:Enabled", "true"),
            ("affolterNET:Web:Mcp:Audience", "emails-mcp"));

        var options = services.AddMcpAuthentication(new AppSettings(true, AuthenticationMode.Authenticate), config);

        Assert.Equal("https://kc.example.com/realms/demo", options.Authority);
    }

    [Fact]
    public void Enabled_without_an_audience_fails_fast()
    {
        var services = new ServiceCollection();
        var config = Config(
            ("affolterNET:Web:Auth:Provider:AuthorityBase", "https://kc.example.com"),
            ("affolterNET:Web:Auth:Provider:Realm", "demo"),
            ("affolterNET:Web:Mcp:Enabled", "true"));

        var ex = Assert.Throws<InvalidOperationException>(
            () => services.AddMcpAuthentication(new AppSettings(true, AuthenticationMode.Authenticate), config));

        Assert.Contains("Audience", ex.Message);
    }

    [Fact]
    public void Enabled_without_an_authority_fails_fast()
    {
        var services = new ServiceCollection();
        var config = Config(
            ("affolterNET:Web:Mcp:Enabled", "true"),
            ("affolterNET:Web:Mcp:Audience", "emails-mcp"));

        var ex = Assert.Throws<InvalidOperationException>(
            () => services.AddMcpAuthentication(new AppSettings(true, AuthenticationMode.Authenticate), config));

        Assert.Contains("AuthorityBase", ex.Message);
    }

    [Fact]
    public void Resource_metadata_advertises_the_authority_and_scopes()
    {
        var options = new McpAuthOptions
        {
            Enabled = true,
            AuthorityBase = "https://kc.example.com",
            Realm = "demo",
            Audience = "emails-mcp",
            RequiredScopes = ["emails.read"],
            ResourceName = "Emails MCP",
        };

        var metadata = ServiceCollectionExtensions.BuildResourceMetadata(options);

        Assert.Null(metadata.Resource); // derived from the request when not configured
        Assert.Equal(["https://kc.example.com/realms/demo"], metadata.AuthorizationServers);
        Assert.Equal(["emails.read"], metadata.ScopesSupported);
        Assert.Equal("Emails MCP", metadata.ResourceName);
    }

    [Fact]
    public void Scopes_supported_can_be_wider_than_the_required_ones()
    {
        var options = new McpAuthOptions
        {
            RequiredScopes = ["emails.read"],
            ScopesSupported = ["emails.read", "emails.write"],
        };

        Assert.Equal(["emails.read", "emails.write"], options.AdvertisedScopes);
    }

    [Theory]
    [InlineData("emails.read profile", true)]
    [InlineData("profile", false)]
    [InlineData("", false)]
    public void Scope_claim_is_split_on_spaces(string scopeClaim, bool expected)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("scope", scopeClaim)], "test"));

        Assert.Equal(expected, ScopeClaims.Satisfies(principal, ["emails.read"]));
    }

    [Fact]
    public void No_required_scopes_accepts_any_token()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([], "test"));

        Assert.True(ScopeClaims.Satisfies(principal, []));
    }
}
