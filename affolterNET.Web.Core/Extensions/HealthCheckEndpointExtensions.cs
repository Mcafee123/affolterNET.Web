using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace affolterNET.Web.Core.Extensions;

public static class HealthCheckEndpointExtensions
{
    /// <summary>
    /// Maps standard startup, liveness, and readiness health check endpoints with OpenAPI documentation.
    /// </summary>
    public static IEndpointRouteBuilder MapStandardHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Startup probe: waits for checks tagged "startup" (Keycloak, app start)
        endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("startup"),
            AllowCachingResponses = false,
        });

        // Liveness probe: simple self-check
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Name == "self",
            AllowCachingResponses = false
        });

        // Readiness probe: checks tagged "ready" or "startup", returns JSON
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready") || registration.Tags.Contains("startup"),
            AllowCachingResponses = false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.TotalMilliseconds
                    })
                };
                await context.Response.WriteAsJsonAsync(result);
            }
        });

        return endpoints;
    }
}