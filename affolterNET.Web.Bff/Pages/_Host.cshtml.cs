using affolterNET.Web.Bff.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace affolterNET.Web.Bff.Pages;

// ReSharper disable once InconsistentNaming
[AllowAnonymous]
public class _HostModel(
    IHostEnvironment hostEnvironment,
    IOptionsMonitor<BffOptions> bffOptions,
    ILogger<_HostModel> logger)
    : PageModel
{
    public bool IsDev => hostEnvironment.IsDevelopment();
    public string FrontendUrl => bffOptions.CurrentValue.FrontendUrl;
    public string Version => Environment.GetEnvironmentVariable("CONTAINER_IMAGE_VERSION") ?? "LOCAL DEBUGGING...";
    public string Env => IsDev ? "DEV" : "PROD";

    public void OnGet()
    {
        logger.LogInformation("Logging from Index PageModel");
    }
}