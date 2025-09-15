using Microsoft.Extensions.Configuration;

namespace affolterNET.Auth.Api.Configuration;

/// <summary>
/// JWT Bearer token validation configuration options
/// </summary>
public class JwtBearerOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Api:JwtBearer";

    /// <summary>
    /// Creates default configuration for JWT Bearer authentication
    /// </summary>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>JwtBearerOptions with environment-appropriate defaults</returns>
    public static JwtBearerOptions CreateDefaults(bool isDevelopment = false)
    {
        return new JwtBearerOptions
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireHttpsMetadata = !isDevelopment, // Allow HTTP in development
            SaveToken = false, // Don't save tokens in AuthenticationProperties for APIs
            IncludeErrorDetails = isDevelopment, // Include detailed error info in development
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes of clock skew
        };
    }

    /// <summary>
    /// Whether to validate the token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the token audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate token lifetime (expiration)
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate the issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Whether to require HTTPS metadata endpoints
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Whether to save the token in AuthenticationProperties
    /// </summary>
    public bool SaveToken { get; set; } = false;

    /// <summary>
    /// Whether to include error details in authentication failures
    /// </summary>
    public bool IncludeErrorDetails { get; set; } = false;

    /// <summary>
    /// Allowed clock skew for token validation
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Valid audiences for token validation (optional, uses client ID if not specified)
    /// </summary>
    public string[] ValidAudiences { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Valid issuers for token validation (optional, uses Authority if not specified)
    /// </summary>
    public string[] ValidIssuers { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Expected token type (default: "JWT")
    /// </summary>
    public string TokenType { get; set; } = "JWT";
}