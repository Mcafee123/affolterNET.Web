using System.Reflection;
using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Extensions;

public static class LocalSecretsExtensions
{
    public static IConfigurationBuilder AddLocalSecrets(this IConfigurationBuilder builder, string environment)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine assembly directory");
        var secretsDir = Path.Combine(assemblyDirectory, ".secrets");
        
        // Add shared secrets file first (lower priority)
        var sharedSecretsPath = Path.Combine(secretsDir, "affolterNET.Bexio.json");
        builder.Add(new LocalSecretsConfigurationSource(sharedSecretsPath));
        
        // Add environment-specific secrets file (higher priority - will override shared values)
        var environmentSecretsFileName = environment.ToLowerInvariant() switch
        {
            "development" => "affolterNET.Bexio.development.json",
            "production" => "affolterNET.Bexio.production.json",
            _ => $"affolterNET.Bexio.{environment.ToLowerInvariant()}.json"
        };
        
        var environmentSecretsPath = Path.Combine(secretsDir, environmentSecretsFileName);
        builder.Add(new LocalSecretsConfigurationSource(environmentSecretsPath));
        
        return builder;
    }
}