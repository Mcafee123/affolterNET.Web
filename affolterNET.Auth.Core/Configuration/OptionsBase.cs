namespace affolterNET.Auth.Core.Configuration;

[Obsolete("remove if not used anymore")]
public abstract class OptionsBase
{
    protected readonly Dictionary<string, object?> Options = new();
    
    public string GetString(string key, string defaultValue)
    {
        var val = GetOption(key, false);
        if (val == null)
        {
            return defaultValue;
        }

        return val.ToString() ?? throw new InvalidOperationException("should not be possible - val is never null");
    }
    
    public string GetString(string key)
    {
        var val = GetOption(key, true);
        return val!.ToString() ?? throw new InvalidOperationException("should not be possible - val is never null");
    }

    public int GetInt(string key, int defaultValue)
    {
        var val = GetOption(key, false);
        if (val == null)
        {
            return defaultValue;
        }

        return Convert.ToInt32(val);
    }
    
    public int GetInt(string key)
    {
        var val = GetOption(key, true);
        return Convert.ToInt32(val);
    }
    
    public bool GetBool(string key, bool defaultValue)
    {
        var val = GetOption(key, false);
        if (val == null)
        {
            return defaultValue;
        }

        return Convert.ToBoolean(val);
    }
    
    public bool GetBool(string key)
    {
        var val = GetOption(key, true);
        return Convert.ToBoolean(val);
    }

    private object? GetOption(string key, bool throwIfMissing)
    {
        if (!Options.ContainsKey(key) && throwIfMissing)
        {
            throw new InvalidOperationException($"Setting \"{key}\" not found in application settings");
        }

        return Options.GetValueOrDefault(key);
    }
}