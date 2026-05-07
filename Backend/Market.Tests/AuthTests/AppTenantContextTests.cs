using System.Security.Claims;
using Market.Infrastructure.Common;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace Market.Tests.AuthTests;

public sealed class AppTenantContextTests
{
    [Fact]
    public void TenantId_ReturnsNull_WhenHttpContextIsMissing()
    {
        var context = new AppTenantContext(new HttpContextAccessor());

        Assert.Null(context.TenantId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void TenantId_ReturnsNull_WhenClaimIsMissingOrInvalid(string? tenantClaim)
    {
        var context = CreateContext(tenantClaim);

        Assert.Null(context.TenantId);
    }

    [Fact]
    public void TenantId_ReturnsClaimValue_WhenClaimIsValid()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateContext(tenantId.ToString());

        Assert.Equal(tenantId, context.TenantId);
    }

    [Fact]
    public void IsSuperAdmin_ReturnsTrue_WhenRoleClaimMatches()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Role, RoleNames.SuperAdmin) },
                "test"))
        };

        var context = new AppTenantContext(new HttpContextAccessor { HttpContext = httpContext });

        Assert.True(context.IsSuperAdmin);
    }

    private static AppTenantContext CreateContext(string? tenantClaim)
    {
        var claims = new List<Claim>();
        if (tenantClaim is not null)
        {
            claims.Add(new Claim("tenant_id", tenantClaim));
        }

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        };

        return new AppTenantContext(new HttpContextAccessor { HttpContext = httpContext });
    }
}
