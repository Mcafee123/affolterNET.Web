using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using affolterNET.Auth.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.Models.Tokens;

namespace affolterNET.Auth.Core.Services;

public class RptCacheService
{
    private readonly IMemoryCache _cache;
    private readonly TokenHelper _tokenHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthConfiguration _authConfig;

    public RptCacheService(
        IMemoryCache cache,
        TokenHelper tokenHelper,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AuthConfiguration> authConfig)
    {
        _cache = cache;
        _tokenHelper = tokenHelper;
        _httpContextAccessor = httpContextAccessor;
        _authConfig = authConfig.Value;
    }

    public JwtSecurityToken StoreRpt(KcIdentityProviderToken rpt)
    {
        var token = rpt.AccessToken;
        var decodedToken = _tokenHelper.DecodeToken(token);
        var expiration = _authConfig.Rpt.EnableCaching 
            ? _authConfig.Rpt.CacheExpiration 
            : TimeSpan.FromSeconds(rpt.ExpiresIn);
        _cache.Set(GetKey(), decodedToken, expiration);
        return decodedToken;
    }

    public JwtSecurityToken? GetRpt()
    {
        if (_cache.TryGetValue(GetKey(), out JwtSecurityToken? rpt))
        {
            return rpt;
        }

        return null;
    }

    private string GetKey()
    {
        return $"RPT_{_authConfig.ClientId}_{GetUserId()}";
    }

    private string GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     throw new InvalidOperationException("User ID is missing");
        return userId;
    }

    public void RemoveByUserId()
    {
        _cache.Remove(GetKey());
    }
}