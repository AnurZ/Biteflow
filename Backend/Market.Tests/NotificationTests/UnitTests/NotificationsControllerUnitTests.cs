using System.Reflection;
using Market.API.Controllers;
using Market.Domain.Entities.Notifications;

namespace Market.Tests.NotificationTests.UnitTests;

public class NotificationsControllerUnitTests
{
    private static readonly MethodInfo MatchesTargetMethod =
        typeof(NotificationsController).GetMethod(
            "MatchesTarget",
            BindingFlags.NonPublic | BindingFlags.Static)!;

    [Fact]
    public void MatchesTarget_ShouldReturnTrue_WhenTargetUserMatchesIgnoringCase()
    {
        // Arrange
        var notification = new NotificationEntity { TargetUserId = "User-1" };
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Kitchen" };
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
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Kitchen" };

        var result = (bool)MatchesTargetMethod.Invoke(
            null,
            new object[] { notification, "other-user", roles })!;

        Assert.True(result);
    }
}
