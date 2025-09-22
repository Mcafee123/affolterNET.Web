using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

public class LocalSecretsConfigurationSource(string secretsPath) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new LocalSecretsConfigurationProvider(secretsPath);
    }
}