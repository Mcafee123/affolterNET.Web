using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.HttpClients.Abstraction;
using NETCore.Keycloak.Client.Models.Tokens;

namespace affolterNET.Web.Core.Services;

public class RptTokenService(
    IKeycloakClient keycloakClient,
    IOptionsMonitor<RptOptions> rptOptions,
    IOptionsMonitor<AuthProviderOptions> authProviderOptions,
    ILogger<RptTokenService> logger)
{
    private readonly RptOptions _rptConfig = rptOptions.CurrentValue;
    private readonly AuthProviderOptions _authProviderConfig = authProviderOptions.CurrentValue;

    public string Audience
    {
        get
        {
            var rptAudience = _rptConfig.Audience;
            return string.IsNullOrEmpty(rptAudience) ? _authProviderConfig.ClientId : rptAudience;
        }
    }

    public string Realm => _authProviderConfig.Realm;

    public async Task<KcIdentityProviderToken?> GetRptTokenAsync(string accessToken)
    {
        logger.LogDebug("Fetching RPT token for realm={Realm}, audience={Audience}", Realm, Audience);
        var response = await keycloakClient.Auth.GetRequestPartyTokenAsync(Realm, accessToken, Audience);
        if (response.IsError)
        {
            logger.LogWarning("Error fetching RPT: {Error}", response.ErrorMessage);
            return null;
        }

        logger.LogDebug("RPT token fetched successfully");
        return response.Response;
    }
}