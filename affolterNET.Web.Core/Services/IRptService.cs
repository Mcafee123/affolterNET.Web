using affolterNET.Web.Core.Models;

namespace affolterNET.Web.Core.Services;

public interface IRptService
{
    Task<RptResponse?> GetRptAsync(string accessToken, string? audience = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> ExtractPermissionsFromRptAsync(string rptToken, CancellationToken cancellationToken = default);
}

public class RptResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public int RefreshExpiresIn { get; set; }
}