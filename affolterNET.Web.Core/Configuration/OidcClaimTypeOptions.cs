using affolterNET.Web.Core.Models;
using affolterNET.Web.Core.Options;

namespace affolterNET.Web.Core.Configuration;

/// <summary>
/// Core OIDC protocol configuration options
/// </summary>
public class OidcClaimTypeOptions : IConfigurableOptions<OidcClaimTypeOptions>
{
    public static string SectionName { get; } = "affolterNET:Web:Oidc:ClaimTypes";
    public static OidcClaimTypeOptions CreateDefaults(AppSettings settings)
    {
        return new OidcClaimTypeOptions(settings);
    }

    public void CopyTo(OidcClaimTypeOptions target)
    {
        target.Subject = Subject;
        target.Name = Name;
        target.GivenName = GivenName;
        target.FamilyName = FamilyName;
        target.Email = Email;
        target.EmailVerified = EmailVerified;
        target.PreferredUsername = PreferredUsername;
        target.Roles = Roles;
        target.Groups = Groups;
        target.ResourceAccess = ResourceAccess;
        target.RealmAccess = RealmAccess;
    }
    
    /// <summary>
    /// Parameterless constructor for options pattern compatibility
    /// </summary>
    public OidcClaimTypeOptions() : this(new AppSettings())
    {
    }
    
    /// <summary>
    /// Constructor with AppSettings parameter for meaningful defaults
    /// </summary>
    /// <param name="settings">Application settings containing environment and authentication mode</param>
    private OidcClaimTypeOptions(AppSettings settings)
    {
        Subject = "sub";
        Name = "name";
        GivenName = "given_name";
        FamilyName = "family_name";
        Email = "email";
        EmailVerified = "email_verified";
        PreferredUsername = "preferred_username";
        Roles = "roles";
        Groups = "groups";
        ResourceAccess = "resource_access";
        RealmAccess = "realm_access";
    }
    
    /// <summary>
    /// The claim type for subject identifier (user ID)
    /// </summary>
    public string Subject { get; set; }
    
    /// <summary>
    /// The claim type for full name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The claim type for given name (first name)
    /// </summary>
    public string GivenName { get; set; }
    
    /// <summary>
    /// The claim type for family name (last name)
    /// </summary>
    public string FamilyName { get; set; }
    
    /// <summary>
    /// The claim type for email address
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// The claim type for email verification status
    /// </summary>
    public string EmailVerified { get; set; }
    
    /// <summary>
    /// The claim type for preferred username
    /// </summary>
    public string PreferredUsername { get; set; }
    
    /// <summary>
    /// The claim type for roles
    /// </summary>
    public string Roles { get; set; }
    
    /// <summary>
    /// The claim type for groups
    /// </summary>
    public string Groups { get; set; }
    
    /// <summary>
    /// The claim type for Keycloak resource access (contains client-specific roles)
    /// </summary>
    public string ResourceAccess { get; set; }
    
    /// <summary>
    /// The claim type for Keycloak realm access (contains realm-wide roles)
    /// </summary>
    public string RealmAccess { get; set; }
}