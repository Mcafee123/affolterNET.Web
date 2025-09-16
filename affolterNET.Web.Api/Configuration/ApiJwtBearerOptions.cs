using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Options;

namespace affolterNET.Web.Api.Configuration;

/// <summary>
/// JWT Bearer token validation configuration options
/// </summary>
public class ApiJwtBearerOptions: IConfigurableOptions<ApiJwtBearerOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Api:JwtBearer";

    public static ApiJwtBearerOptions CreateDefaults(AppSettings appSettings)
    {
        return new ApiJwtBearerOptions(appSettings);
    }

    public void CopyTo(ApiJwtBearerOptions target)
    {
        target.ValidateIssuer = ValidateIssuer;
        target.ValidateAudience = ValidateAudience;
        target.ValidateLifetime = ValidateLifetime;
        target.ValidateIssuerSigningKey = ValidateIssuerSigningKey;
        target.RequireHttpsMetadata = RequireHttpsMetadata;
        target.SaveToken = SaveToken;
        target.IncludeErrorDetails = IncludeErrorDetails;
        target.ClockSkew = ClockSkew;
        target.ValidAudiences = ValidAudiences;
        target.ValidIssuers = ValidIssuers;
        target.TokenType = TokenType;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public ApiJwtBearerOptions() : this(new AppSettings())
    {
    }

    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="appSettings"></param>
    private ApiJwtBearerOptions(AppSettings appSettings)
    {
        ValidateIssuer = true;
        ValidateAudience = true;
        ValidateLifetime = true;
        ValidateIssuerSigningKey = true;
        RequireHttpsMetadata = !appSettings.IsDev; // Allow HTTP in development
        SaveToken = false; // Don't save tokens in AuthenticationProperties for APIs
        IncludeErrorDetails = appSettings.IsDev; // Include detailed error info in development
        ClockSkew = TimeSpan.FromMinutes(5); // Allow 5 minutes of clock skew
        ValidAudiences = [];
        ValidIssuers = [];
        TokenType = "JWT";
    }

    /// <summary>
    /// Whether to validate the token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; }

    /// <summary>
    /// Whether to validate the token audience
    /// </summary>
    public bool ValidateAudience { get; set; }

    /// <summary>
    /// Whether to validate token lifetime (expiration)
    /// </summary>
    public bool ValidateLifetime { get; set; }

    /// <summary>
    /// Whether to validate the issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; }

    /// <summary>
    /// Whether to require HTTPS metadata endpoints
    /// </summary>
    public bool RequireHttpsMetadata { get; set; }

    /// <summary>
    /// Whether to save the token in AuthenticationProperties
    /// </summary>
    public bool SaveToken { get; set; }

    /// <summary>
    /// Whether to include error details in authentication failures
    /// </summary>
    public bool IncludeErrorDetails { get; set; }

    /// <summary>
    /// Allowed clock skew for token validation
    /// </summary>
    public TimeSpan ClockSkew { get; set; }

    /// <summary>
    /// Valid audiences for token validation (optional, uses client ID if not specified)
    /// </summary>
    public string[] ValidAudiences { get; set; }

    /// <summary>
    /// Valid issuers for token validation (optional, uses Authority if not specified)
    /// </summary>
    public string[] ValidIssuers { get; set; }

    /// <summary>
    /// Expected token type (default: "JWT")
    /// </summary>
    public string TokenType { get; set; }
}