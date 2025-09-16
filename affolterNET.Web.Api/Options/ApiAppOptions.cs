using System.Text.Json;
using System.Text.Json.Serialization;
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
        // ApiJwtBearerOptions still uses old pattern, so create it manually for now
        ApiJwtBearer = ApiJwtBearerOptions.CreateDefaults(appSettings);
        config.GetSection(ApiJwtBearerOptions.SectionName).Bind(ApiJwtBearer);
    }

    public ApiJwtBearerOptions ApiJwtBearer { get; set; }
    public Action<ApiJwtBearerOptions>? ConfigureApiJwtBearer { get; set; }

    public void Configure(IServiceCollection services)
    {
        ConfigureCore(services);
        ApiJwtBearer.Configure(services, ConfigureApiJwtBearer);
    }

    /// <summary>
    /// Serializes the ApiAppOptions to JSON string for logging purposes
    /// </summary>
    /// <returns>JSON representation of the configuration</returns>
    public string ToJson()
    {
        var result = new Dictionary<string, object>();
        ApiJwtBearer.AddToConfigurationDict(result);
        AuthProvider.AddToConfigurationDict(result);
        Oidc.AddToConfigurationDict(result);
        AuthProvider.AddToConfigurationDict(result);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Serialize(result, options);
    }
}