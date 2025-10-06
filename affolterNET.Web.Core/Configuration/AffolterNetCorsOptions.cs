using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Options;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// CORS (Cross-Origin Resource Sharing) configuration options
/// </summary>
public class AffolterNetCorsOptions : IConfigurableOptions<AffolterNetCorsOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET:Web:Cors";

    public static AffolterNetCorsOptions CreateDefaults(AppSettings settings)
    {
        return new AffolterNetCorsOptions(settings);
    }

    public void CopyTo(AffolterNetCorsOptions target)
    {
        target.Enabled = Enabled;
        target.AllowedOrigins = new List<string>(AllowedOrigins);
        target.AllowCredentials = AllowCredentials;
        target.AllowedMethods = new List<string>(AllowedMethods);
        target.AllowedHeaders = new List<string>(AllowedHeaders);
        target.ExposedHeaders = new List<string>(ExposedHeaders);
        target.MaxAge = MaxAge;
        target.PolicyName = PolicyName;
    }

    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public AffolterNetCorsOptions(): this(new AppSettings())
    {
    }

    /// <summary>
    /// Constructor with AppSettings for default configuration
    /// </summary>
    private AffolterNetCorsOptions(AppSettings settings)
    {
        Enabled = false;
        AllowedOrigins = []; // Must be explicitly configured
        AllowedMethods = ["GET", "POST", "PUT", "DELETE", "OPTIONS"];
        AllowedHeaders = ["Content-Type", "Authorization", "Accept", "Origin", "X-Requested-With"];
        AllowCredentials = false;
        MaxAge = 7200;
        if (settings.IsDev)
        {
            AllowedOrigins =
            [
                "https://localhost:4200",
                "https://localhost:7163",
                "https://localhost:7164",
                "http://localhost:8081"
            ];
            AllowedMethods = ["*"];
            AllowedHeaders = ["*"];
            AllowCredentials = false;
            MaxAge = 3600;
        }
    }

    /// <summary>
    /// Whether CORS middleware is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// List of allowed origins for CORS requests.
    /// Use "*" to allow all origins (not recommended for production with credentials).
    /// </summary>
    public List<string> AllowedOrigins { get; set; }

    /// <summary>
    /// Whether to allow credentials (cookies, authorization headers, TLS client certificates) in CORS requests.
    /// Cannot be used with AllowedOrigins containing "*".
    /// </summary>
    public bool AllowCredentials { get; set; } = false;

    /// <summary>
    /// List of allowed HTTP methods for CORS requests.
    /// Use "*" to allow all methods.
    /// </summary>
    public List<string> AllowedMethods { get; set; }

    /// <summary>
    /// List of allowed request headers for CORS requests.
    /// Use "*" to allow all headers.
    /// </summary>
    public List<string> AllowedHeaders { get; set; }

    /// <summary>
    /// List of response headers to expose to the client.
    /// Use "*" to expose all headers (with some exceptions).
    /// </summary>
    public List<string> ExposedHeaders { get; set; } = [];

    /// <summary>
    /// Maximum age (in seconds) for preflight request caching.
    /// </summary>
    public int MaxAge { get; set; }

    /// <summary>
    /// Name of the CORS policy. If not specified, uses the default policy.
    /// </summary>
    public string PolicyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Validates the CORS configuration
    /// </summary>
    public void Validate(bool isDevelopment)
    {
        if (!Enabled)
        {
            return;
        }
        
        // Check for conflicting settings
        if (AllowCredentials && AllowedOrigins.Contains("*"))
        {
            throw new InvalidOperationException(
                "CORS configuration error: Cannot allow credentials (*) when AllowedOrigins contains '*'. " +
                "Specify explicit origins or set AllowCredentials to false.");
        }

        // Warn about production configuration
        if (!isDevelopment && AllowedOrigins.Count == 0)
        {
            throw new InvalidOperationException(
                "CORS configuration error: No allowed origins specified for production environment. " +
                "Please configure AllowedOrigins in your appsettings.json.");
        }

        // Warn about insecure production settings
        if (!isDevelopment && AllowedOrigins.Contains("*"))
        {
            throw new InvalidOperationException(
                "CORS configuration error: Wildcard origins (*) not recommended for production. " +
                "Please specify explicit origins for security.");
        }
    }
}