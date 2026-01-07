using Microsoft.Extensions.Configuration;

namespace affolterNET.Web.Core.Configuration;

[Obsolete("Don't use this anymore, stick to standard secrets approaches")]
public class LocalSecretsConfigurationSource(string secretsPath) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new LocalSecretsConfigurationProvider(secretsPath);
    }
}