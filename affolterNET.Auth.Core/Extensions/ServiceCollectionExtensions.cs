using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using affolterNET.Auth.Core.Configuration;

namespace affolterNET.Auth.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthCoreOptions>(configuration.GetSection(AuthCoreOptions.SectionName));
        
        // Core services will be registered by specific implementations (API/BFF libraries)
        
        return services;
    }
}