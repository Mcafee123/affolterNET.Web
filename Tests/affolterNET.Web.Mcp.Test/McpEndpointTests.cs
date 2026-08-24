using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using affolterNET.Web.Core.Models;
using affolterNET.Web.Mcp.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace affolterNET.Web.Mcp.Test;

/// <summary>
/// Drives the real pipeline: an unauthenticated MCP request must produce the
/// discovery chain an MCP client walks (401 → WWW-Authenticate → metadata document).
/// No Keycloak is contacted — without a token the JWT handler never fetches metadata.
/// </summary>
public class McpEndpointTests : IAsyncLifetime
{
    private const string Authority = "https://kc.example.com/realms/demo";

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
            ["affolterNET:Web:Mcp:Enabled"] = "true",
            ["affolterNET:Web:Mcp:Audience"] = "demo-mcp",
            ["affolterNET:Web:Mcp:RequiredScopes:0"] = "demo.read",
        });

        builder.Services.AddMcpServer().WithHttpTransport().WithTools<PingTool>();
        builder.Services.AddMcpAuthentication(
            new AppSettings(true, AuthenticationMode.Authenticate), builder.Configuration);

        _app = builder.Build();
        _app.UseRouting();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapMcpSecured();

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Unauthenticated_request_is_rejected_and_points_at_the_metadata_document()
    {
        var response = await _client.PostAsJsonAsync("/mcp", new { jsonrpc = "2.0", id = 1, method = "initialize" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var challenge = Assert.Single(response.Headers.WwwAuthenticate);
        Assert.Equal("Bearer", challenge.Scheme);
        Assert.Contains("resource_metadata=\"http://localhost/.well-known/oauth-protected-resource/mcp\"",
            challenge.Parameter);
    }

    [Fact]
    public async Task Metadata_document_names_the_authorization_server_and_scopes()
    {
        var response = await _client.GetAsync("/.well-known/oauth-protected-resource/mcp");

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.Equal("http://localhost/mcp", root.GetProperty("resource").GetString());
        Assert.Equal(Authority, root.GetProperty("authorization_servers")[0].GetString());
        Assert.Equal("demo.read", root.GetProperty("scopes_supported")[0].GetString());
    }

    [Fact]
    public async Task A_bogus_token_does_not_pass()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = JsonContent.Create(new { jsonrpc = "2.0", id = 1, method = "initialize" }),
        };
        request.Headers.Add("Authorization", "Bearer not-a-jwt");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [McpServerToolType]
    private sealed class PingTool
    {
        [McpServerTool(Name = "ping")]
        [Description("Test tool.")]
        public static string Ping() => "pong";
    }
}
