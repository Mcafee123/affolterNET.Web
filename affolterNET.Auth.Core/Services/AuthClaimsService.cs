using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace affolterNET.Auth.Core.Services;

public class AuthClaimsService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RptTokenService _rptTokenService;
    private readonly RptCacheService _rptCacheService;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public AuthClaimsService(
        IHttpContextAccessor httpContextAccessor,
        RptTokenService rptTokenService,
        RptCacheService rptCacheService)
    {
        _httpContextAccessor = httpContextAccessor;
        _rptTokenService = rptTokenService;
        _rptCacheService = rptCacheService;
    }

    public async Task EnrichUserWithPermissionsAndRoles(string accessToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var rptToken = await GetRptToken(accessToken);
        if (rptToken == null)
        {
            return;
        }

        var authorizationClaim = rptToken.Claims.FirstOrDefault(c => c.Type == "authorization");
        if (authorizationClaim == null)
        {
            return;
        }

        var permissions = ExtractPermissionsFromJwt(authorizationClaim.Value);

        // Add permissions as claims
        var identity = (ClaimsIdentity)httpContext.User.Identity;
        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim("permission", permission));
        }
    }

    private static List<string> ExtractPermissionsFromJwt(string authorizationClaimValue)
    {
        var permissions = new List<string>();

        try
        {
            using var document = JsonDocument.Parse(authorizationClaimValue);
            var root = document.RootElement;

            if (root.TryGetProperty("permissions", out var permissionsElement))
            {
                foreach (var permission in permissionsElement.EnumerateArray())
                {
                    if (permission.TryGetProperty("rsname", out var resourceName))
                    {
                        permissions.Add(resourceName.GetString() ?? string.Empty);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Log error or handle gracefully
        }

        return permissions;
    }

    private async Task<JwtSecurityToken?> GetRptToken(string accessToken)
    {
        // get from cache
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null;
        var rpt = _rptCacheService.GetRpt(userId);
        if (rpt == null)
        {
            await RefreshLock.WaitAsync();
            try
            {
                // check again
                rpt = _rptCacheService.GetRpt(userId);
                if (rpt == null)
                {
                    // get token from keycloak
                    var token = await _rptTokenService.GetRptTokenAsync(accessToken);
                    if (token == null)
                    {
                        // could not get rpt, access_token may be timed out
                        return null;
                    }

                    // token received, store in cache as JwtSecurityToken
                    var handler = new JwtSecurityTokenHandler();
                    if (!handler.CanReadToken(token.AccessToken))
                        return null;
                    var decodedToken = handler.ReadJwtToken(token.AccessToken);
                    rpt = _rptCacheService.StoreRpt(userId, token, decodedToken);
                }
            }
            finally
            {
                RefreshLock.Release();
            }
        }
        
        return rpt;
    }
}