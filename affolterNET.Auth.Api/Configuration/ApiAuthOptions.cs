using affolterNET.Auth.Core.Configuration;

namespace affolterNET.Auth.Api.Configuration;

public class ApiAuthOptions : AuthCoreOptions
{
    public new const string SectionName = "ApiAuth";
    
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool SaveToken { get; set; } = true;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
    public JwtValidationOptions Validation { get; set; } = new();
}

public class JwtValidationOptions
{
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public string[] ValidIssuers { get; set; } = Array.Empty<string>();
    public string[] ValidAudiences { get; set; } = Array.Empty<string>();
}