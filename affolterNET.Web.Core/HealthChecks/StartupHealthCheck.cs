using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace affolterNET.Web.Core.HealthChecks;

public class StartupHealthCheck(StartupStatusService status) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return status.IsStarted
            ? Task.FromResult(HealthCheckResult.Healthy("Application started"))
            : Task.FromResult(HealthCheckResult.Unhealthy("Application still starting"));
    }
}

