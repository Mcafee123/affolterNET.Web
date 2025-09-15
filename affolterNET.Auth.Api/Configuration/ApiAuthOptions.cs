using affolterNET.Auth.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace affolterNET.Auth.Api.Configuration;

/// <summary>
/// API authentication configuration options for backend services and APIs
/// </summary>
public class ApiAuthOptions : AuthProviderOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public const string SectionName = "affolterNET.Auth:Api";

    /// <summary>
    /// Creates default configuration for API authentication
    /// </summary>
    /// <param name="config">Optional configuration to bind values from</param>
    /// <returns>ApiAuthOptions instance with defaults applied</returns>
    public static ApiAuthOptions CreateDefaults(IConfiguration? config = null)
    {
        var options = new ApiAuthOptions();
        
        if (config != null)
        {
            config.GetSection(SectionName).Bind(options);
        }
        
        return options;
    }

    /// <summary>
    /// API version for versioned endpoints
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Base URL for the API service
    /// </summary>
    public string ApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// SPA application URL for CORS and redirect purposes
    /// </summary>
    public string SpaUrl { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URI for OAuth2/OIDC flows in API scenarios
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Expected audience for JWT token validation
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// OIDC protocol configuration
    /// </summary>
    public OidcOptions Oidc { get; set; } = new();

    /// <summary>
    /// JWT Bearer token validation options
    /// </summary>
    public JwtBearerOptions JwtBearer { get; set; } = new();
}