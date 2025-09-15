using System.Security.Claims;
using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Core.Services;

public interface IPermissionService
{
    Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(string userId, string accessToken, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(string userId, string resource, string action, CancellationToken cancellationToken = default);
    Task InvalidateUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
}

public interface IClaimsEnrichmentService
{
    Task<UserContext> EnrichUserContextAsync(ClaimsPrincipal principal, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<ClaimsPrincipal> EnrichClaimsAsync(ClaimsPrincipal principal, string? accessToken = null, CancellationToken cancellationToken = default);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}