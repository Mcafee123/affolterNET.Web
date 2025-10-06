using affolterNET.Web.Api.Configuration;
using affolterNET.Web.Core.Options;
using affolterNET.Web.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Web.Api.Options;

public class ApiAppOptions : CoreAppOptions
{
    public ApiAppOptions(AppSettings appSettings, IConfiguration config) : base(appSettings, config)
    {
        ApiJwtBearer = ApiJwtBearerOptions.CreateDefaults(appSettings);
    }

    public ApiJwtBearerOptions ApiJwtBearer { get; set; }
    public Action<ApiJwtBearerOptions>? ConfigureApiJwtBearer { get; set; }

    public void Configure(IServiceCollection services)
    {
        var actions = new ConfigureActions();
        actions.Add(ConfigureApiJwtBearer);
        
        ApiJwtBearer.RunActions(actions);
        
        RunCoreActions();
        ConfigureCoreDi(services);
    }

    public void ValidateConfiguration()
    {
        // nothing validated yet
    }

    protected override Dictionary<string, object> GetConfigs()
    {
        var configDict = new Dictionary<string, object>();
        ApiJwtBearer.AddToConfigurationDict(configDict);
        return configDict;
    }
}