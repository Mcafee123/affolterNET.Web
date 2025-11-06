namespace affolterNET.Web.Core.HealthChecks;

public class StartupStatusService
{
    private volatile bool _started;
    public bool IsStarted => _started;
    public void MarkStarted() => _started = true;
}
