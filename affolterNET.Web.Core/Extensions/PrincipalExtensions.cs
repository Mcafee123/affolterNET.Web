using System.Security.Claims;

namespace affolterNET.Web.Core.Extensions;

public static class PrincipalExtensions
{
  public static string FindFirstClaimValue(this ClaimsPrincipal principal, string claimType)
  {
    return principal.FindFirst(claimType)?.Value ?? string.Empty;
  }
}