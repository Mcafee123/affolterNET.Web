using System.Globalization;
using affolterNET.Auth.Core.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.HttpClients.Abstraction;
using NETCore.Keycloak.Client.Models.Auth;
using NETCore.Keycloak.Client.Models.Tokens;

namespace affolterNET.Auth.Core.Services;

/// <summary>
/// Service for refreshing authentication tokens using Keycloak refresh tokens
/// </summary>
public class TokenRefreshService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IKeycloakClient _keycloakClient;
    private readonly ILogger<TokenRefreshService> _logger;
    private readonly string _realm;
    private readonly KcClientCredentials _clientCredentials;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public TokenRefreshService(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AuthConfiguration> authConfig,
        IKeycloakClient keycloakClient,
        ILogger<TokenRefreshService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _keycloakClient = keycloakClient;
        _logger = logger;
        _realm = authConfig.Value.Realm;
        _clientCredentials = new KcClientCredentials
        {
            ClientId = authConfig.Value.ClientId,
            Secret = authConfig.Value.ClientSecret
        };
    }

    /// <summary>
    /// Refreshes the authentication tokens if they are expired or about to expire
    /// </summary>
    /// <returns>True if tokens were refreshed successfully, false otherwise</returns>
    public async Task<bool> RefreshTokensAsync()
    {
        await RefreshLock.WaitAsync();
        try
        {
            // Check again after acquiring the lock
            if (!await IsExpired())
            {
                return true;
            }

            // Request new tokens from Keycloak
            _logger.LogDebug("Refreshing tokens...");
            var refreshToken = await GetRefreshToken();
            if (refreshToken == null)
            {
                _logger.LogWarning("Refreshing token failed - no refresh token available");
                return false;
            }
            _logger.LogDebug("Refresh token received from context");
            
            var newTokens = await RequestNewTokensAsync(refreshToken);
            if (newTokens == null)
            {
                _logger.LogDebug("Requesting new tokens failed");
                return false;
            }

            // Extract new tokens from Keycloak response
            var newAccessToken = newTokens.AccessToken;
            var newRefreshToken = newTokens.RefreshToken;
            var expiresIn = newTokens.ExpiresIn;

            // Calculate new absolute expiration time
            var newExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            
            // Update the tokens
            return await UpdateAuthTokensAsync(newAccessToken, newRefreshToken, newExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refreshing tokens failed");
            return false;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    /// <summary>
    /// Gets the expiration time of the current access token
    /// </summary>
    /// <returns>The expiration time or null if not available</returns>
    public async Task<DateTime?> ExpiresAt()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            _logger.LogError("HttpContext is null");
            return null;
        }

        var expiresAtString = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at");
        if (!DateTime.TryParse(expiresAtString, null, DateTimeStyles.AdjustToUniversal, out DateTime expiresAt))
        {
            return null;
        }

        return expiresAt;
    }

    /// <summary>
    /// Checks if the current access token is expired or about to expire
    /// </summary>
    /// <param name="secondsBeforeExpiration">Number of seconds before expiration to consider the token expired</param>
    /// <returns>True if the token is expired or about to expire</returns>
    public async Task<bool> IsExpired(int secondsBeforeExpiration = 10)
    {
        var expiresAt = await ExpiresAt();
        if (expiresAt == null)
        {
            return true;
        }

        var nowUtc = DateTime.UtcNow;
        var isExpired = expiresAt <= nowUtc ||
                        expiresAt.Value.Subtract(nowUtc) < TimeSpan.FromSeconds(secondsBeforeExpiration);
        return isExpired;
    }

    private async Task<string?> GetRefreshToken()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            _logger.LogWarning("HttpContext is null");
            return null;
        }

        var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");
        return refreshToken;
    }

    private async Task<KcIdentityProviderToken?> RequestNewTokensAsync(string refreshToken)
    {
        // Send refresh token request to Keycloak
        _logger.LogDebug("Refreshing tokens...");
        var kcResponse = await _keycloakClient.Auth.RefreshAccessTokenAsync(_realm, _clientCredentials, refreshToken);
        if (kcResponse.IsError)
        {
            _logger.LogError("Refreshing tokens failed: {Error}", kcResponse.ErrorMessage);
            return null;
        }

        return kcResponse.Response;
    }

    private async Task<bool> UpdateAuthTokensAsync(string newAccessToken, string newRefreshToken,
        DateTime newExpiresAt)
    {
        // Get current auth ticket (cookie) and update the token values
        if (_httpContextAccessor.HttpContext == null)
        {
            return false;
        }
        var context = _httpContextAccessor.HttpContext;
        var currentAuth =
            await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = currentAuth.Properties;
        if (authProperties == null)
        {
            _logger.LogError("Authentication properties are missing");
            throw new InvalidOperationException("Authentication properties are missing");
        }

        authProperties.UpdateTokenValue("access_token", newAccessToken);
        authProperties.UpdateTokenValue("refresh_token", newRefreshToken);
        authProperties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));

        // Re-issue the authentication cookie with the new tokens
        if (currentAuth.Principal == null)
        {
            _logger.LogError("Authentication principal is missing");
            throw new InvalidOperationException("Authentication principal is missing");
        }

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            currentAuth.Principal, authProperties);
        _logger.LogDebug("Tokens refreshed and saved");
        return true;
    }
}