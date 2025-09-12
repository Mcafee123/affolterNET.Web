using System.IdentityModel.Tokens.Jwt;

namespace affolterNET.Auth.Core.Services;

public class TokenHelper
{
    public JwtSecurityToken DecodeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            throw new InvalidOperationException("cannot decode token");
        }

        var jwt = handler.ReadJwtToken(token);
        return jwt;
    }
}