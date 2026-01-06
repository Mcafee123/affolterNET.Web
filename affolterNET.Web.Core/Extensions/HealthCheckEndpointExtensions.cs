using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace affolterNET.Web.Core.Extensions;

public static class HealthCheckEndpointExtensions
{
    /// <summary>
    /// Route order for health check endpoints. Uses a very low value to ensure health checks
    /// have higher priority than YARP catch-all routes (which typically use Order: 1000+).
    /// </summary>
    private const int HealthCheckRouteOrder = -1000;

    /// <summary>
    /// Maps standard startup, liveness, and readiness health check endpoints with OpenAPI documentation.
    /// Health check endpoints are registered with high priority (low Order value) to ensure they
    /// are matched before YARP reverse proxy catch-all routes.
    /// </summary>
    public static IEndpointRouteBuilder MapStandardHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Startup probe: waits for checks tagged "startup" (Keycloak, app start)
        endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("startup"),
            AllowCachingResponses = false,
        }).WithOrder(HealthCheckRouteOrder);

        // Liveness probe: simple self-check
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Name == "self",
            AllowCachingResponses = false
        }).WithOrder(HealthCheckRouteOrder);

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
        }).WithOrder(HealthCheckRouteOrder);

        return endpoints;
    }
}