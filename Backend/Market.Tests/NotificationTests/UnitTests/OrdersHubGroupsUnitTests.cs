using Market.API.Hubs;
using Market.Shared.Constants;

namespace Market.Tests.NotificationTests.UnitTests;

public class OrdersHubGroupsUnitTests
{
    [Fact]
    public void Groups_ShouldScopeDefaultTenantId()
    {
        var tenantId = SeedConstants.DefaultTenantId;

        Assert.Equal($"tenant:{tenantId}:kitchen", OrdersHubGroups.Kitchen(tenantId));
        Assert.Equal($"tenant:{tenantId}:waiter", OrdersHubGroups.Waiter(tenantId));
        Assert.Equal($"tenant:{tenantId}:admin", OrdersHubGroups.Admin(tenantId));
        Assert.Equal($"tenant:{tenantId}:role:{RoleNames.Kitchen}", OrdersHubGroups.Role(RoleNames.Kitchen, tenantId));
        Assert.Equal($"tenant:{tenantId}:user:user-1", OrdersHubGroups.User("user-1", tenantId));
    }

    [Fact]
    public void Groups_ShouldNotScopeEmptyTenantId()
    {
        Assert.Equal(string.Empty, OrdersHubGroups.Kitchen(Guid.Empty));
        Assert.Equal(string.Empty, OrdersHubGroups.Waiter(Guid.Empty));
        Assert.Equal(string.Empty, OrdersHubGroups.Admin(Guid.Empty));
        Assert.Equal(string.Empty, OrdersHubGroups.Role(RoleNames.Kitchen, Guid.Empty));
        Assert.Equal(string.Empty, OrdersHubGroups.User("user-1", Guid.Empty));
    }

    [Fact]
    public void Groups_ShouldNotScopeMissingTenantId()
    {
        Assert.Equal(string.Empty, OrdersHubGroups.Kitchen(null));
        Assert.Equal(string.Empty, OrdersHubGroups.Waiter(null));
        Assert.Equal(string.Empty, OrdersHubGroups.Admin(null));
        Assert.Equal(string.Empty, OrdersHubGroups.Role(RoleNames.Kitchen, null));
        Assert.Equal(string.Empty, OrdersHubGroups.User("user-1", null));
    }
}
