namespace affolterNET.Auth.Bff.Models;

/// <summary>
/// Authorization modes for the BFF application
/// </summary>
public enum AuthorizationMode
{
    /// <summary>
    /// No authentication required - anonymous access
    /// </summary>
    None,
    
    /// <summary>
    /// Authentication required but no permission checks
    /// </summary>
    AuthenticatedOnly,
    
    /// <summary>
    /// Full permission-based authorization with claims
    /// </summary>
    PermissionBased
}