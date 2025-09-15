using affolterNET.Web.Api.Configuration;
using affolterNET.Web.Core.Options;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Api.Options;

public class ApiAppOptions : CoreAppOptions
{
    public ApiAppOptions(bool isDev) : base(isDev)
    {
        ApiJwtBearer = new ApiJwtBearerOptions(isDev);
    }

    public ApiJwtBearerOptions ApiJwtBearer { get; set; }
    
    public void Configure(IServiceCollection services)
    {
        services.Configure<ApiJwtBearerOptions>(options => ApiJwtBearer.CopyTo(options));      
    }
}