using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using affolterNET.Auth.Core.Models;

namespace affolterNET.Auth.Bff.Services;

public interface IBffSessionService
{
    Task<bool> IsUserAuthenticatedAsync(HttpContext context);
    Task<UserContext?> GetUserContextAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task<string?> GetAccessTokenAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task RefreshTokensAsync(HttpContext context, CancellationToken cancellationToken = default);
    Task RevokeTokensAsync(HttpContext context, CancellationToken cancellationToken = default);
}

public interface IBffApiClient
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string endpoint, string? accessToken = null, CancellationToken cancellationToken = default) where T : class;
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string? accessToken = null, CancellationToken cancellationToken = default) 
        where TRequest : class 
        where TResponse : class;
}

/// <summary>
/// Optional service for custom token cache cleanup during logout
/// </summary>
public interface ITokenCacheCleanupService
{
    void ClearUserTokens();
}