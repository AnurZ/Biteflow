using System.Security.Claims;
using System.Reflection;
using Market.API.Controllers;
using Market.Domain.Entities.Notifications;
using Market.Shared.Constants;

namespace Market.Tests.NotificationTests.UnitTests;

public class NotificationsControllerUnitTests
{
    private static readonly MethodInfo MatchesTargetMethod =
        typeof(NotificationsController).GetMethod(
            "MatchesTarget",
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo ResolveRolesMethod =
        typeof(NotificationsController).GetMethod(
            "ResolveRoles",
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo ResolveTenantIdMethod =
        typeof(NotificationsController).GetMethod(
            "ResolveTenantId",
            BindingFlags.NonPublic | BindingFlags.Static)!;

    [Fact]
    public void MatchesTarget_ShouldReturnTrue_WhenTargetUserMatchesIgnoringCase()
    {
        // Arrange
        var notification = new NotificationEntity { TargetUserId = "User-1" };
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { RoleNames.Kitchen };
        var userId = "user-1";

        var result = (bool)MatchesTargetMethod.Invoke(
            null,
            new object[] { notification, userId, roles })!;

        Assert.True(result);
    }

    [Fact]
    public void MatchesTarget_ShouldReturnTrue_WhenRoleMatchesIgnoringCase()
    {
        var notification = new NotificationEntity { TargetRole = "kitchen" };
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { RoleNames.Kitchen };

        var result = (bool)MatchesTargetMethod.Invoke(
            null,
            new object[] { notification, "other-user", roles })!;

        Assert.True(result);
    }

    [Fact]
    public void ResolveRoles_ShouldIgnorePositionClaims_WhenRoleClaimIsMissing()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("position", "Kitchen")
        }));

        var roles = (HashSet<string>)ResolveRolesMethod.Invoke(
            null,
            new object[] { user })!;

        Assert.Empty(roles);
    }

    [Fact]
    public void ResolveRoles_ShouldReturnRoleClaims_CaseInsensitiveForTargetMatching()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RoleNames.Kitchen)
        }));
        var roles = (HashSet<string>)ResolveRolesMethod.Invoke(
            null,
            new object[] { user })!;
        var notification = new NotificationEntity { TargetRole = "KITCHEN" };

        var result = (bool)MatchesTargetMethod.Invoke(
            null,
            new object[] { notification, "other-user", roles })!;

        Assert.True(result);
    }

    [Fact]
    public void ResolveTenantId_ShouldReturnDefaultTenantId()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant_id", SeedConstants.DefaultTenantId.ToString())
        }));

        var tenantId = (Guid?)ResolveTenantIdMethod.Invoke(
            null,
            new object[] { user });

        Assert.Equal(SeedConstants.DefaultTenantId, tenantId);
    }
}
