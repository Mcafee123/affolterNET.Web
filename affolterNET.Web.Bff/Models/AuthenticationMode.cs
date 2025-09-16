namespace affolterNET.Web.Bff.Models;

/// <summary>
/// Authentication modes for the BFF application
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// No authentication required - anonymous access
    /// </summary>
    None,
    
    /// <summary>
    /// Authentication required but no permission checks
    /// </summary>
    Authenticate,
    
    /// <summary>
    /// Full permission-based authorization with claims
    /// </summary>
    Authorize
}