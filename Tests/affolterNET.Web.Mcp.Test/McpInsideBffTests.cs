using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using affolterNET.Web.Bff.Extensions;
using affolterNET.Web.Core.Models;
using affolterNET.Web.Mcp.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace affolterNET.Web.Mcp.Test;

/// <summary>
/// The MCP endpoint lives inside a cookie-based BFF. Its antiforgery, its SPA
/// fallback and its login redirect must all leave the MCP route alone — an MCP
/// client has no CSRF cookie and cannot follow a browser login.
/// </summary>
public class McpInsideBffTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["affolterNET:Web:Auth:Provider:AuthorityBase"] = "https://kc.example.com",
            ["affolterNET:Web:Auth:Provider:Realm"] = "demo",
            ["affolterNET:Web:Auth:Provider:ClientId"] = "demo-bff",
            ["affolterNET:Web:Auth:Provider:ClientSecret"] = "not-used-in-this-test",
            ["affolterNET:Web:Mcp:Enabled"] = "true",
            ["affolterNET:Web:Mcp:Audience"] = "demo-mcp",
        });

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var appSettings = new AppSettings(false, AuthenticationMode.Authenticate);
        var bffOptions = builder.Services.AddBffServices(appSettings, builder.Configuration, options =>
        {
            options.ConfigureBff = bff =>
            {
                bff.EnableHttpsRedirection = false;
                // The real BFF falls back to the Razor page _Host, which a test host has
                // no Razor SDK for. MapFallback below stands in for it: same catch-all
                // precedence, so "does the SPA fallback swallow /mcp" is still answered.
                bff.FallbackPage = null;
            };
        });

        builder.Services.AddMcpServer().WithHttpTransport().WithTools<EchoTool>();
        builder.Services.AddMcpAuthentication(appSettings, builder.Configuration);

        _app = builder.Build();
        _app.ConfigureBffApp(bffOptions);
        _app.MapMcpSecured();
        _app.MapFallback(() => Results.Text("spa-shell"));

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Mcp_request_gets_a_401_challenge_not_a_login_redirect()
    {
        var response = await _client.PostAsJsonAsync("/mcp", new { jsonrpc = "2.0", id = 1, method = "initialize" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("resource_metadata=", Assert.Single(response.Headers.WwwAuthenticate).Parameter);
    }

    [Fact]
    public async Task Antiforgery_does_not_block_the_cookieless_client()
    {
        // A POST without the __Host- CSRF cookie must fail on authentication (401),
        // never on antiforgery (400) — the endpoint opts out of it.
        var response = await _client.PostAsJsonAsync("/mcp", new { jsonrpc = "2.0", id = 1, method = "initialize" });

        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Spa_fallback_does_not_swallow_the_mcp_route()
    {
        var response = await _client.PostAsJsonAsync("/mcp", new { jsonrpc = "2.0", id = 1, method = "initialize" });

        Assert.NotEqual("spa-shell", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Metadata_document_is_served_from_inside_the_bff_pipeline()
    {
        var response = await _client.GetAsync("/.well-known/oauth-protected-resource/mcp");

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("https://kc.example.com/realms/demo",
            doc.RootElement.GetProperty("authorization_servers")[0].GetString());
    }

    [McpServerToolType]
    private sealed class EchoTool
    {
        [McpServerTool(Name = "echo")]
        [Description("Test tool.")]
        public static string Echo(string text) => text;
    }
}
