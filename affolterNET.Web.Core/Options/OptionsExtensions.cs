using affolterNET.Web.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace affolterNET.Web.Core.Options;

public static class OptionsExtensions
{
    /// <summary>
    /// Create with defaults + bind from config
    /// </summary>
    /// <param name="config"></param>
    /// <param name="appSettings"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CreateFromConfig<T>(this IConfiguration config, AppSettings appSettings)
        where T : class, IConfigurableOptions<T>
    {
        var options = T.CreateDefaults(appSettings);
        config.GetSection(T.SectionName).Bind(options);
        return options;
    }

    /// <summary>
    /// Apply manual configuration
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T RunActions<T>(this T options, ConfigureActions configure)
        where T : class, IConfigurableOptions<T>
    {
        foreach (var act in configure.GetActions<T>())
        {
            ((Action<T>)act).Invoke(options);
        }
        return options;
    }
    
    /// <summary>
    /// Register with DI
    /// </summary>
    /// <param name="options"></param>
    /// <param name="services"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ConfigureDi<T>(this T options, IServiceCollection services)
        where T : class, IConfigurableOptions<T>
    {
        services.Configure<T>(options.CopyTo);
        return options;
    }
    
    /// <summary>
    /// Create Config Dictionary
    /// </summary>
    /// <param name="option"></param>
    /// <param name="result"></param>
    /// <typeparam name="T"></typeparam>
    public static void AddToConfigurationDict<T>(this IConfigurableOptions<T> option, Dictionary<string, object> result)
        where T : class, IConfigurableOptions<T>
    {
        var parts = T.SectionName.Split(':').Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (parts.Count == 0) return;
        
        // Build nested structure: "affolterNET.Web:Bff:Options" -> result["affolterNET.Web"]["Bff"]["Options"]
        var current = result;
        for (int i = 0; i < parts.Count - 1; i++)
        {
            var key = parts[i];
            if (!current.ContainsKey(key))
            {
                current[key] = new Dictionary<string, object>();
            }
            current = (Dictionary<string, object>)current[key];
        }
        
        // Add the properties at the final level
        var finalKey = parts[^1];
        current[finalKey] = option.GetPublicProperties();
    }
    
    /// <summary>
    /// Gets all public properties of an object as a dictionary for JSON serialization
    /// </summary>
    /// <param name="obj">The object to extract properties from</param>
    /// <returns>Dictionary of property names and values</returns>
    private static Dictionary<string, object?> GetPublicProperties(this object obj)
    {
        var properties = obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !IsExcludedProperty(p))
            .ToDictionary(
                p => p.Name,
                p => GetPropertyValue(p, obj)
            );

        return properties;
    }

    private static bool IsExcludedProperty(PropertyInfo property)
    {
        // Exclude read-only properties (no setter) - these won't appear in config
        if (!property.CanWrite)
            return true;
            
        // Exclude Action delegates and other non-serializable types
        return typeof(Delegate).IsAssignableFrom(property.PropertyType) ||
               property.PropertyType.Name.StartsWith("Action") ||
               property.PropertyType.Name.StartsWith("Func") ||
               property.GetCustomAttribute<System.ComponentModel.EditorBrowsableAttribute>()?.State ==
               System.ComponentModel.EditorBrowsableState.Never;
    }

    private static object? GetPropertyValue(PropertyInfo property, object? obj)
    {
        try
        {
            var value = property.GetValue(obj);
            return value;
        }
        catch
        {
            return null;
        }
    }
}