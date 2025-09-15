using affolterNET.Web.Core.Configuration;

namespace affolterNET.Web.Core.Options;

public abstract class CoreAppOptions
{
    public CoreAppOptions(bool isDev)
    {
        AuthProvider = new AuthProviderOptions(isDev);
        Oidc = new OidcOptions(isDev);
        PermissionCache = new PermissionCacheOptions(isDev);
        SecurityHeaders = new SecurityHeadersOptions(isDev);
        Swagger = new SwaggerOptions(isDev);
    }

    public AuthProviderOptions AuthProvider { get; set; }

    public OidcOptions Oidc { get; set; }

    public PermissionCacheOptions PermissionCache { get; set; }

    /// <summary>
    /// Whether to enable security headers middleware
    /// </summary>
    public bool EnableSecurityHeaders { get; set; } = true;

    public SecurityHeadersOptions SecurityHeaders { get; set; }

    public SwaggerOptions Swagger { get; set; }
}