using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace affolterNET.Web.Core.HealthChecks;

public class KeycloakHealthCheck(IHttpClientFactory http, IOptions<AuthProviderOptions> authProviderOptions) : IHealthCheck
{
    private readonly IHttpClientFactory _http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly string _realm = authProviderOptions.Value.Realm;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _http.CreateClient("keycloak");
            var url = $"realms/{_realm}/.well-known/openid-configuration";
            using var res = await client.GetAsync(url, cancellationToken);

            if (res.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Keycloak reachable");
            }

            return HealthCheckResult.Unhealthy($"Keycloak responded {(int)res.StatusCode}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Keycloak health check canceled");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Keycloak health check failed: {ex.Message}");
        }
    }
}

