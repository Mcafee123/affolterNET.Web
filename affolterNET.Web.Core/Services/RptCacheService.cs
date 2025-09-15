using System.IdentityModel.Tokens.Jwt;
using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.Models.Tokens;

namespace affolterNET.Web.Core.Services;

public class RptCacheService(
    IMemoryCache cache,
    IOptionsMonitor<AuthProviderOptions> authProviderOptions,
    IOptionsMonitor<RptOptions> rptOptions)
{
    private readonly AuthProviderOptions _oidcOptions = authProviderOptions.CurrentValue;
    private readonly RptOptions _rptConfig = rptOptions.CurrentValue;

    public JwtSecurityToken StoreRpt(string userId, KcIdentityProviderToken rpt, JwtSecurityToken decodedToken)
    {
        var expiration = _rptConfig.EnableCaching 
            ? _rptConfig.CacheExpiration 
            : TimeSpan.FromSeconds(rpt.ExpiresIn);
        cache.Set(GetKey(userId), decodedToken, expiration);
        return decodedToken;
    }

    public JwtSecurityToken? GetRpt(string userId)
    {
        if (cache.TryGetValue(GetKey(userId), out JwtSecurityToken? rpt))
        {
            return rpt;
        }
        return null;
    }

    private string GetKey(string userId)
    {
        return $"RPT_{_oidcOptions.ClientId}_{userId}";
    }

    public void RemoveByUserId(string userId)
    {
        cache.Remove(GetKey(userId));
    }
}