using affolterNET.Web.Core.Options;

namespace affolterNET.Web.Core.Configuration;

public class SwaggerOptions: IConfigurableOptions<SwaggerOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Swagger";

    public static bool IsEmpty(SwaggerOptions options)
    {
        throw new NotImplementedException();
    }

    public static Action<SwaggerOptions>? GetConfigureAction()
    {
        throw new NotImplementedException();
    }

    public void CopyTo(SwaggerOptions target)
    {
        target.Title = Title;
        target.Version = Version;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public SwaggerOptions() : this(false)
    {
    }

    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public SwaggerOptions(bool isDev)
    {
        Title = "API Documentation";
        Version = "v1";
    }

    public string Title { get; set; }
    public string Version { get; set; }
}