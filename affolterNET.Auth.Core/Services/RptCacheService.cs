using System.IdentityModel.Tokens.Jwt;
using affolterNET.Auth.Core.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.Models.Tokens;

namespace affolterNET.Auth.Core.Services;

public class RptCacheService
{
    private readonly IMemoryCache _cache;
    private readonly AuthConfiguration _authConfig;

    public RptCacheService(
        IMemoryCache cache,
        IOptions<AuthConfiguration> authConfig)
    {
        _cache = cache;
        _authConfig = authConfig.Value;
    }

    public JwtSecurityToken StoreRpt(string userId, KcIdentityProviderToken rpt, JwtSecurityToken decodedToken)
    {
        var expiration = _authConfig.Rpt.EnableCaching 
            ? _authConfig.Rpt.CacheExpiration 
            : TimeSpan.FromSeconds(rpt.ExpiresIn);
        _cache.Set(GetKey(userId), decodedToken, expiration);
        return decodedToken;
    }

    public JwtSecurityToken? GetRpt(string userId)
    {
        if (_cache.TryGetValue(GetKey(userId), out JwtSecurityToken? rpt))
        {
            return rpt;
        }
        return null;
    }

    private string GetKey(string userId)
    {
        return $"RPT_{_authConfig.ClientId}_{userId}";
    }

    public void RemoveByUserId(string userId)
    {
        _cache.Remove(GetKey(userId));
    }
}