using affolterNET.Web.Api.Configuration;
using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Api.Options;

public class ApiAppOptions : CoreAppOptions
{
    public ApiAppOptions(bool isDev, IConfiguration config) : base(isDev, config)
    {
        ApiJwtBearer = config.CreateFromConfig<ApiJwtBearerOptions>(isDev);
    }

    public ApiJwtBearerOptions ApiJwtBearer { get; set; }
    public Action<ApiJwtBearerOptions>? ConfigureApiJwtBearer { get; set; }

    public void Configure(IServiceCollection services)
    {
        ConfigureCore(services);
        ApiJwtBearer.Configure(services, ConfigureApiJwtBearer);
    }
}