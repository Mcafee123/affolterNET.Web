using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace affolterNET.Web.Core.Options;

public class GenericOptionsConfigurator<T>(IConfiguration configuration)
    : IConfigureOptions<T>
    where T : class, IConfigurableOptions<T>, new()
{
    public void Configure(T options)
    {
        // 1. Constructor already set defaults!
        
        // 2. Bind from configuration (overwrites defaults)
        configuration.GetSection(T.SectionName).Bind(options);
        
        // 3. Apply manual overrides
        T.GetConfigureAction()?.Invoke(options);
    }
}
