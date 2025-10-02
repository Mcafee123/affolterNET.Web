using System.Reflection;
using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Extensions;

public static class LocalSecretsExtensions
{
    public static IConfigurationBuilder AddLocalSecrets(this IConfigurationBuilder builder, string environment, string configFile)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine assembly directory");
        var secretsDir = Path.Combine(assemblyDirectory, ".secrets");
        
        // Add shared secrets file first (lower priority)
        var sharedSecretsPath = Path.Combine(secretsDir, configFile);
        builder.Add(new LocalSecretsConfigurationSource(sharedSecretsPath));
        
        // Add environment-specific secrets file (higher priority - will override shared values)
        var fileNameParts = SplitFilename(configFile);
        var environmentSecretsFileName = environment.ToLowerInvariant() switch
        {
            "development" => $"{fileNameParts.Item1}.development{fileNameParts.Item2}",
            "production" => $"{fileNameParts.Item1}.production{fileNameParts.Item2}",
            _ => $"{fileNameParts.Item1}.{environment.ToLowerInvariant()}{fileNameParts.Item2}"
        };
        
        var environmentSecretsPath = Path.Combine(secretsDir, environmentSecretsFileName);
        builder.Add(new LocalSecretsConfigurationSource(environmentSecretsPath));
        
        return builder;
    }

    private static Tuple<string, string> SplitFilename(string fileName)
    {
        var withoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        return new Tuple<string, string>(withoutExtension, extension);
    }
}