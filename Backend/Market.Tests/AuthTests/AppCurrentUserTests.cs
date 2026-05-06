using System.Security.Claims;
using Market.Infrastructure.Common;
using Microsoft.AspNetCore.Http;

namespace Market.Tests.AuthTests;

public sealed class AppCurrentUserTests
{
    [Fact]
    public void UserId_ReturnsNull_WhenHttpContextIsMissing()
    {
        var currentUser = new AppCurrentUser(new HttpContextAccessor());

        Assert.Null(currentUser.UserId);
        Assert.False(currentUser.IsAuthenticated);
    }

    [Fact]
    public void UserId_ReturnsNameIdentifierClaim_WhenClaimIsGuid()
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void UserId_ReturnsSubjectClaim_WhenNameIdentifierIsMissing()
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(new Claim("sub", userId.ToString()));

        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenClaimIsNotGuid()
    {
        var currentUser = CreateCurrentUser(new Claim(ClaimTypes.NameIdentifier, "123"));

        Assert.Null(currentUser.UserId);
    }

    [Fact]
    public void EmailAndIsAuthenticated_ReturnCurrentPrincipalValues()
    {
        var currentUser = CreateCurrentUser(new Claim("email", "user@example.test"));

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal("user@example.test", currentUser.Email);
    }

    private static AppCurrentUser CreateCurrentUser(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        };

        return new AppCurrentUser(new HttpContextAccessor { HttpContext = httpContext });
    }
}
