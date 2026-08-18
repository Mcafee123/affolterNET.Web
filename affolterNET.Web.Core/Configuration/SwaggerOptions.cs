using System.Reflection;
using affolterNET.Web.Core.Options;
using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Core.Configuration;

public class SwaggerOptions: IConfigurableOptions<SwaggerOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET:Web:Swagger";

    public static bool IsEmpty(SwaggerOptions options)
    {
        throw new NotImplementedException();
    }

    public static SwaggerOptions CreateDefaults(AppSettings settings)
    {
        return new SwaggerOptions(settings);
    }

    public void CopyTo(SwaggerOptions target)
    {
        target.EnableSwagger = EnableSwagger;
        target.RequireAuthentication = RequireAuthentication;
        target.Title = Title;
        target.Version = Version;
        target.ShowHealthEndpoints = ShowHealthEndpoints;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public SwaggerOptions() : this(new AppSettings())
    {
    }

    /// <summary>
    /// Constructor with BffAppSettings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing environment and authentication mode</param>
    private SwaggerOptions(AppSettings settings)
    {
        var env = settings.IsDev ? "DEV" : "PROD";
        Title = $"{Assembly.GetEntryAssembly()?.GetName().Name} - {env} - API";
        Version = "v1";
        // Swagger discloses the full API surface - on by default only during development.
        // Production environments opt in explicitly (EnableSwagger = true in config).
        EnableSwagger = settings.IsDev;
        RequireAuthentication = true;
        ShowHealthEndpoints = true;
    }

    public string Title { get; set; }
    public string Version { get; set; }
    public bool EnableSwagger { get; set; }

    /// <summary>
    /// Require a signed-in user for /swagger (default: true). Only effective when an
    /// authentication mode is configured - with AuthenticationMode.None there is no
    /// scheme to authenticate against and Swagger is served without a gate.
    /// </summary>
    public bool RequireAuthentication { get; set; }

    public bool ShowHealthEndpoints { get; set; }
}