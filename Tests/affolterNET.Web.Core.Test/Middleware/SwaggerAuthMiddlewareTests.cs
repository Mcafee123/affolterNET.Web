using System.Security.Claims;
using affolterNET.Web.Core.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace affolterNET.Web.Core.Test.Middleware;

public class SwaggerAuthMiddlewareTests
{
    [Fact]
    public async Task NonSwaggerPath_PassesThroughWithoutAuthenticating()
    {
        var auth = new FakeAuthenticationService(authenticated: false);
        var (middleware, context, nextCalled) = Create(auth, "/api/some/endpoint");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled());
        Assert.False(auth.AuthenticateCalled);
        Assert.False(auth.ChallengeCalled);
    }

    [Fact]
    public async Task SwaggerPath_Unauthenticated_ChallengesAndStops()
    {
        var auth = new FakeAuthenticationService(authenticated: false);
        var (middleware, context, nextCalled) = Create(auth, "/swagger/index.html");

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled());
        Assert.True(auth.ChallengeCalled);
    }

    [Fact]
    public async Task SwaggerPath_Authenticated_SetsUserAndContinues()
    {
        var auth = new FakeAuthenticationService(authenticated: true);
        var (middleware, context, nextCalled) = Create(auth, "/swagger/v1/swagger.json");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled());
        Assert.False(auth.ChallengeCalled);
        Assert.True(context.User.Identity?.IsAuthenticated);
    }

    private static (SwaggerAuthMiddleware middleware, HttpContext context, Func<bool> nextCalled) Create(
        FakeAuthenticationService auth, string path)
    {
        var called = false;
        var middleware = new SwaggerAuthMiddleware(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            NullLogger<SwaggerAuthMiddleware>.Instance);

        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(auth);
        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Path = path;
        return (middleware, context, () => called);
    }

    private class FakeAuthenticationService(bool authenticated) : IAuthenticationService
    {
        public bool AuthenticateCalled { get; private set; }
        public bool ChallengeCalled { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        {
            AuthenticateCalled = true;
            if (!authenticated)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "tester")], "Test");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            ChallengeCalled = true;
            return Task.CompletedTask;
        }

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal,
            AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;
    }
}
