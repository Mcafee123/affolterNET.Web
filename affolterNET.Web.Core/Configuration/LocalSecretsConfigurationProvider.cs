using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace affolterNET.Web.Core.Configuration;

[Obsolete("Don't use this anymore, stick to standard secrets approaches")]
public class LocalSecretsConfigurationProvider(string secretsPath) : ConfigurationProvider
{
    public override void Load()
    {
        if (!File.Exists(secretsPath))
            return;

        try
        {
            var json = File.ReadAllText(secretsPath);
            var secrets = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (secrets != null)
            {
                Data = FlattenJson(secrets);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the application
            Console.WriteLine($"Error loading local secrets: {ex.Message}");
        }
    }

    private IDictionary<string, string?> FlattenJson(Dictionary<string, object> json, string prefix = "")
    {
        var result = new Dictionary<string, string?>();

        foreach (var kvp in json)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}:{kvp.Key}";

            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var nested = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                    if (nested != null)
                    {
                        var flattened = FlattenJson(nested, key);
                        foreach (var item in flattened)
                        {
                            result[item.Key] = item.Value;
                        }
                    }
                }
                else
                {
                    result[key] = element.ToString();
                }
            }
            else
            {
                result[key] = kvp.Value?.ToString();
            }
        }

        return result;
    }
}