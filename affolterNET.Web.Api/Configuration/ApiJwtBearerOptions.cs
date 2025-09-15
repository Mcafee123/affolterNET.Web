using affolterNET.Web.Core.Options;
using Microsoft.Extensions.Configuration;

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

    public static Action<ApiJwtBearerOptions>? GetConfigureAction()
    {
        throw new NotImplementedException();
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
    public ApiJwtBearerOptions() : this(false)
    {
    }
    
    /// <summary>
    /// Constructor with environment parameter for meaningful defaults
    /// </summary>
    /// <param name="isDev">Whether running in development mode</param>
    public ApiJwtBearerOptions(bool isDev)
    {
        ValidateIssuer = true;
        ValidateAudience = true;
        ValidateLifetime = true;
        ValidateIssuerSigningKey = true;
        RequireHttpsMetadata = !isDev; // Allow HTTP in development
        SaveToken = false; // Don't save tokens in AuthenticationProperties for APIs
        IncludeErrorDetails = isDev; // Include detailed error info in development
        ClockSkew = TimeSpan.FromMinutes(5); // Allow 5 minutes of clock skew
        ValidAudiences = Array.Empty<string>();
        ValidIssuers = Array.Empty<string>();
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

    /// <summary>
    /// Static factory method that handles config binding with environment awareness
    /// </summary>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="isDev">Whether running in development mode</param>
    /// <param name="configure">Optional configurator action</param>
    /// <returns>Configured JwtBearerOptions instance</returns>
    public static ApiJwtBearerOptions CreateFromConfiguration(IConfiguration configuration, 
        bool isDev,
        Action<ApiJwtBearerOptions>? configure = null)
    {
        var options = new ApiJwtBearerOptions(isDev);
        configuration.GetSection(SectionName).Bind(options);
        configure?.Invoke(options);
        return options;
    }
}