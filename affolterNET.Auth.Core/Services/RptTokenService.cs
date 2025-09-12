using affolterNET.Auth.Core.Configuration;
using Microsoft.Extensions.Options;
using NETCore.Keycloak.Client.HttpClients.Abstraction;
using NETCore.Keycloak.Client.Models.Tokens;

namespace affolterNET.Auth.Core.Services;

public class RptTokenService
{
    private readonly AuthConfiguration _authConfig;
    private readonly IKeycloakClient _keycloakClient;

    public RptTokenService(IOptions<AuthConfiguration> authConfig, IKeycloakClient keycloakClient)
    {
        _authConfig = authConfig.Value;
        _keycloakClient = keycloakClient;
    }

    public string Audience 
    { 
        get
        {
            var rptAudience = _authConfig.Rpt.Audience;
            return string.IsNullOrEmpty(rptAudience) ? _authConfig.ClientId : rptAudience;
        }
    }
    public string Realm => _authConfig.Realm;
    
    public async Task<KcIdentityProviderToken?> GetRptTokenAsync(string accessToken)
    {
        var response = await _keycloakClient.Auth.GetRequestPartyTokenAsync(Realm, accessToken, Audience);
        if (response.IsError)
        {
            var responseError = response.ErrorMessage;
            Console.WriteLine($"Error fetching RPT: {responseError}");
            return null;
        }
        return response.Response;
    }
}