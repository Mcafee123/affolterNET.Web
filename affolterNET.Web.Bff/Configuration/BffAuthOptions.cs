using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Options;

namespace affolterNET.Web.Bff.Configuration;

public class BffAuthOptions: IConfigurableOptions<BffAuthOptions>
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json
    /// </summary>
    public static string SectionName => "affolterNET.Web:Auth:Options";

    public static BffAuthOptions CreateDefaults(AppSettings settings)
    {
        return new BffAuthOptions(settings);
    }

    public void CopyTo(BffAuthOptions options)
    {
        throw new NotImplementedException();
    }

    public BffAuthOptions(): this(new AppSettings(false, AuthenticationMode.None))
    {
    }

    private BffAuthOptions(AppSettings appSettings)
    {
        CallbackPath = "/signin-oidc";
        SignoutCallback = "/logout-callback";
        PostLogoutRedirectUri = "/signout-callback-oidc";
        RedirectUri = "/signin-oidc";
    }

    /// <summary>
    /// Callback path for OIDC authentication
    /// </summary>
    public string CallbackPath { get; set; }
    
    /// <summary>
    /// Signout callback path
    /// </summary>
    public string SignoutCallback { get; set; }

    /// <summary>
    /// Post logout redirect URI
    /// </summary>
    public string PostLogoutRedirectUri { get; set; }
    
    /// <summary>
    /// Redirect URI for OIDC flows
    /// </summary>
    public string RedirectUri { get; set; }
}