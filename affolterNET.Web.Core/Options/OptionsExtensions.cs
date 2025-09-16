using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Core.Options;

public static class OptionsExtensions
{
    // Create with defaults + bind from config
    public static T CreateFromConfig<T>(this IConfiguration config, bool isDev)
        where T : class, IConfigurableOptions<T>
    {
        var options = T.CreateDefaults(isDev);
        config.GetSection(T.SectionName).Bind(options);
        return options;
    }
    
    // Apply manual configuration and register with DI
    public static T Configure<T>(this T options, IServiceCollection services, Action<T>? configure)
        where T : class, IConfigurableOptions<T>
    {
        configure?.Invoke(options);
        services.Configure<T>(options.CopyTo);
        return options;
    }
}
