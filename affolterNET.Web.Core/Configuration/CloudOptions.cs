using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Options;

namespace affolterNET.Web.Core.Configuration;

public class CloudOptions: IConfigurableOptions<CloudOptions>
{
    public static string SectionName { get; } = "affolterNET:Web:Cloud";
    public static CloudOptions CreateDefaults(AppSettings settings)
    {
        return new CloudOptions(settings);
    }
    
    public CloudOptions() : this(new AppSettings())
    {
    }

    public void CopyTo(CloudOptions target)
    {
        target.MapHealthChecks = MapHealthChecks;
    }
    
    private CloudOptions(AppSettings settings)
    {
        MapHealthChecks = true;
    }

    public bool MapHealthChecks { get; set; }
}