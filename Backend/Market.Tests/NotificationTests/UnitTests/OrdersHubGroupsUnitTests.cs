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
        Assert.Equal($"tenant:{tenantId}:role:{RoleNames.Kitchen}", OrdersHubGroups.Role(RoleNames.Kitchen, tenantId));
        Assert.Equal($"tenant:{tenantId}:user:user-1", OrdersHubGroups.User("user-1", tenantId));
    }

    [Fact]
    public void Groups_ShouldNotScopeEmptyTenantId()
    {
        Assert.Equal("kitchen", OrdersHubGroups.Kitchen(Guid.Empty));
        Assert.Equal("waiter", OrdersHubGroups.Waiter(Guid.Empty));
        Assert.Equal($"role:{RoleNames.Kitchen}", OrdersHubGroups.Role(RoleNames.Kitchen, Guid.Empty));
        Assert.Equal("user:user-1", OrdersHubGroups.User("user-1", Guid.Empty));
    }

    [Fact]
    public void Groups_ShouldNotScopeMissingTenantId()
    {
        Assert.Equal("kitchen", OrdersHubGroups.Kitchen(null));
        Assert.Equal("waiter", OrdersHubGroups.Waiter(null));
        Assert.Equal($"role:{RoleNames.Kitchen}", OrdersHubGroups.Role(RoleNames.Kitchen, null));
        Assert.Equal("user:user-1", OrdersHubGroups.User("user-1", null));
    }
}
