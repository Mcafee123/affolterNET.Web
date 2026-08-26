using affolterNET.Web.Bff.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace affolterNET.Web.Bff.Test;

public class BffControllerTests
{
    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("/", "/")]
    [InlineData("/dashboard?tab=history", "/dashboard?tab=history")]
    public void Login_uses_local_return_url(string? returnUrl, string expectedRedirectUri)
    {
        var result = Assert.IsType<ChallengeResult>(CreateController().Login(returnUrl));
        var properties = Assert.IsType<AuthenticationProperties>(result.Properties);

        Assert.Equal(expectedRedirectUri, properties.RedirectUri);
    }

    [Theory]
    [InlineData("https://attacker.example")]
    [InlineData("//attacker.example")]
    public void Login_rejects_non_local_return_url(string returnUrl)
    {
        var result = Assert.IsType<ChallengeResult>(CreateController().Login(returnUrl));
        var properties = Assert.IsType<AuthenticationProperties>(result.Properties);

        Assert.Equal("/", properties.RedirectUri);
    }

    private static BffController CreateController()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ControllerActionDescriptor());

        return new BffController(null!)
        {
            ControllerContext = new ControllerContext(actionContext),
            Url = new UrlHelper(actionContext)
        };
    }
}
