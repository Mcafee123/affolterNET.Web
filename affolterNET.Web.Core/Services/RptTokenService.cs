using affolterNET.Web.Core.Configuration;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.HttpClients.Abstraction;
using NETCore.Keycloak.Client.Models.Tokens;

namespace affolterNET.Web.Core.Services;

public class RptTokenService(
    IKeycloakClient keycloakClient,
    IOptionsMonitor<RptOptions> rptOptions,
    IOptionsMonitor<AuthProviderOptions> authProviderOptions)
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
        var response = await keycloakClient.Auth.GetRequestPartyTokenAsync(Realm, accessToken, Audience);
        if (response.IsError)
        {
            var responseError = response.ErrorMessage;
            Console.WriteLine($"Error fetching RPT: {responseError}");
            return null;
        }

        return response.Response;
    }
}