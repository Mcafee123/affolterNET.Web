namespace affolterNET.Auth.Core.Models;

public class UserContext
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyList<Permission> Permissions { get; set; } = Array.Empty<Permission>();
    public IDictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
}

public class Permission
{
    public string Resource { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
}

public class TokenData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public DateTime RefreshExpiresAt { get; set; }
    public IReadOnlyList<string> Scopes { get; set; } = Array.Empty<string>();
}