using Microsoft.Extensions.Hosting;

namespace affolterNET.Web.Core.HealthChecks;

public class StartupMarker(IHostApplicationLifetime lifetime, StartupStatusService status)
    : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
    private readonly StartupStatusService _status = status ?? throw new ArgumentNullException(nameof(status));

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(() => _status.MarkStarted());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
