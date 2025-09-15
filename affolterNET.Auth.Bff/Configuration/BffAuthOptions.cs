using affolterNET.Auth.Core.Configuration;

namespace affolterNET.Auth.Bff.Configuration;

public class BffAuthOptions : AuthConfiguration
{
    public new const string SectionName = "Auth";
    
    public string PostLogoutRedirectUri { get; set; } = "/signout-callback-oidc";
    
    public CookieAuthOptions Cookie { get; set; } = new();
    public OidcOptions Oidc { get; set; } = new();
    public BffOptions Bff { get; set; } = new();
}

public class CookieAuthOptions
{
    public string Name { get; set; } = "__Host-bff";
    public bool HttpOnly { get; set; } = true;
    public bool Secure { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromHours(8);
    public bool SlidingExpiration { get; set; } = true;
}

public class OidcOptions
{
    public string ResponseType { get; set; } = "code";
    public bool SaveTokens { get; set; } = true;
    public bool UsePkce { get; set; } = true;
    public string[] ResponseModes { get; set; } = { "query" };
}

public class BffOptions
{
    public bool EnableSessionManagement { get; set; } = true;
    public string ManagementBasePath { get; set; } = "/bff";
    public bool RequireLogoutSessionId { get; set; } = false;
    public bool RevokeRefreshTokenOnLogout { get; set; } = true;
    public bool BackchannelLogoutAllUserSessions { get; set; } = false;
}