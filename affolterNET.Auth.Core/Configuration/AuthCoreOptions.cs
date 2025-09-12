namespace affolterNET.Auth.Core.Configuration;

public class AuthCoreOptions
{
    public const string SectionName = "AuthCore";
    
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public CacheOptions Cache { get; set; } = new();
    public RptOptions Rpt { get; set; } = new();
}

public class CacheOptions
{
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan PermissionCacheExpiration { get; set; } = TimeSpan.FromMinutes(10);
    public int MaxCacheSize { get; set; } = 1000;
}

public class RptOptions
{
    public string Endpoint { get; set; } = "/realms/{realm}/protocol/openid_connect/token";
    public string Audience { get; set; } = string.Empty;
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(10);
}