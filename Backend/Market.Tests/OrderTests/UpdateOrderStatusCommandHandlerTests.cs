using System.ComponentModel.DataAnnotations;
using Market.Application.Abstractions;
using Market.Application.Common.Exceptions;
using Market.Application.Modules.Orders.Commands.UpdateOrderStatus;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Notifications;
using Market.Domain.Entities.Orders;
using Market.Shared.Constants;
using Microsoft.Extensions.Time.Testing;

namespace Market.Tests.OrderTests;

public sealed class UpdateOrderStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAllowKitchenPrepTransitions()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var order = await SeedOrderAsync(db, tenantId, OrderStatus.New);
        var handler = CreateHandler(db, RoleNames.Kitchen);

        var cooking = await handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = OrderStatus.Cooking
        }, CancellationToken.None);

        Assert.True(cooking.StatusChanged);
        Assert.Equal(OrderStatus.Cooking, cooking.Status);
        Assert.Null(cooking.CreatedNotification);

        var ready = await handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = OrderStatus.ReadyForPickup
        }, CancellationToken.None);

        Assert.True(ready.StatusChanged);
        Assert.Equal(OrderStatus.ReadyForPickup, ready.Status);
        Assert.Equal(OrderStatus.ReadyForPickup, (await db.Orders.SingleAsync(x => x.Id == order.Id)).Status);
    }

    [Fact]
    public async Task Handle_ShouldAllowWaiterCompletion()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var order = await SeedOrderAsync(db, tenantId, OrderStatus.ReadyForPickup);
        var handler = CreateHandler(db, RoleNames.Waiter);

        var result = await handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = OrderStatus.Completed
        }, CancellationToken.None);

        Assert.True(result.StatusChanged);
        Assert.Equal(OrderStatus.Completed, result.Status);
        Assert.Equal(OrderStatus.Completed, (await db.Orders.SingleAsync(x => x.Id == order.Id)).Status);
    }

    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Cooking)]
    [InlineData(OrderStatus.New, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cooking, OrderStatus.ReadyForPickup)]
    [InlineData(OrderStatus.Cooking, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.ReadyForPickup, OrderStatus.Completed)]
    [InlineData(OrderStatus.ReadyForPickup, OrderStatus.Cancelled)]
    public async Task Handle_ShouldAllowAdminForAnyValidTransition(OrderStatus current, OrderStatus next)
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var order = await SeedOrderAsync(db, tenantId, current);
        var handler = CreateHandler(db, RoleNames.Admin);

        var result = await handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = next
        }, CancellationToken.None);

        Assert.True(result.StatusChanged);
        Assert.Equal(next, result.Status);
        Assert.Equal(next, (await db.Orders.SingleAsync(x => x.Id == order.Id)).Status);
    }

    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Cooking, RoleNames.Staff)]
    [InlineData(OrderStatus.New, OrderStatus.Cooking, RoleNames.Waiter)]
    [InlineData(OrderStatus.ReadyForPickup, OrderStatus.Completed, RoleNames.Kitchen)]
    [InlineData(OrderStatus.New, OrderStatus.Cooking, RoleNames.SuperAdmin)]
    public async Task Handle_ShouldDenyValidTransitionForWrongRole(OrderStatus current, OrderStatus next, string role)
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var order = await SeedOrderAsync(db, tenantId, current);
        var handler = CreateHandler(db, role);

        await Assert.ThrowsAsync<MarketForbiddenException>(() => handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = next
        }, CancellationToken.None));

        Assert.Equal(current, (await db.Orders.SingleAsync(x => x.Id == order.Id)).Status);
    }

    [Fact]
    public async Task Handle_ShouldRejectInvalidStateTransition()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var order = await SeedOrderAsync(db, tenantId, OrderStatus.New);
        var handler = CreateHandler(db, RoleNames.Admin);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = OrderStatus.Completed
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCreateReadyNotificationWithOrderId()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var order = await SeedOrderAsync(db, tenantId, OrderStatus.Cooking, tableNumber: 12);
        var handler = CreateHandler(db, RoleNames.Kitchen);

        var result = await handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = OrderStatus.ReadyForPickup
        }, CancellationToken.None);

        var notification = await db.Notifications.SingleAsync(x => x.OrderId == order.Id);
        Assert.NotNull(result.CreatedNotification);
        Assert.Equal(notification.Id, result.CreatedNotification.Id);
        Assert.Equal(order.Id, notification.OrderId);
        Assert.Equal(RoleNames.Waiter, notification.TargetRole);
        Assert.Equal("OrderReady", notification.Type);
        Assert.Equal($"/waiter/orders/{order.Id}", notification.Link);
        Assert.Contains("Sto 12", notification.Message);
    }

    [Fact]
    public async Task Handle_ShouldClearOnlyMatchingOrderNotificationsOnCompletion()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var order = await SeedOrderAsync(db, tenantId, OrderStatus.ReadyForPickup);
        var otherOrder = await SeedOrderAsync(db, tenantId, OrderStatus.ReadyForPickup);

        var matchingCreated = CreateNotification(tenantId, order.Id, "OrderCreated", RoleNames.Kitchen);
        var matchingReady = CreateNotification(tenantId, order.Id, "OrderReady", RoleNames.Waiter);
        var otherType = CreateNotification(tenantId, order.Id, "OtherType", RoleNames.Waiter);
        var otherOrderNotification = CreateNotification(tenantId, otherOrder.Id, "OrderReady", RoleNames.Waiter);
        var otherTenantNotification = CreateNotification(otherTenantId, order.Id, "OrderCreated", RoleNames.Kitchen);

        db.Notifications.AddRange(
            matchingCreated,
            matchingReady,
            otherType,
            otherOrderNotification,
            otherTenantNotification);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db, RoleNames.Waiter);

        var result = await handler.Handle(new UpdateOrderStatusCommand
        {
            Id = order.Id,
            Status = OrderStatus.Completed
        }, CancellationToken.None);

        Assert.Equal(new[] { matchingCreated.Id, matchingReady.Id }.OrderBy(x => x), result.ClearedNotificationIds.OrderBy(x => x));

        var notifications = await db.Notifications
            .IgnoreQueryFilters()
            .Where(x => new[]
            {
                matchingCreated.Id,
                matchingReady.Id,
                otherType.Id,
                otherOrderNotification.Id,
                otherTenantNotification.Id
            }.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        Assert.NotNull(notifications[matchingCreated.Id].ReadAtUtc);
        Assert.NotNull(notifications[matchingReady.Id].ReadAtUtc);
        Assert.Null(notifications[otherType.Id].ReadAtUtc);
        Assert.Null(notifications[otherOrderNotification.Id].ReadAtUtc);
        Assert.Null(notifications[otherTenantNotification.Id].ReadAtUtc);
    }

    private static UpdateOrderStatusCommandHandler CreateHandler(
        IAppDbContext db,
        params string[] roles)
        => new(db, new TestCurrentUser(roles));

    private static DatabaseContext CreateContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new DatabaseContext(options, new FakeTimeProvider(), tenantContext);
    }

    private static async Task<Order> SeedOrderAsync(
        DatabaseContext db,
        Guid tenantId,
        OrderStatus status,
        int tableNumber = 5)
    {
        var order = new Order
        {
            TenantId = tenantId,
            Status = status,
            TableNumber = tableNumber
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    private static NotificationEntity CreateNotification(Guid tenantId, int orderId, string type, string role)
        => new()
        {
            TenantId = tenantId,
            OrderId = orderId,
            TargetRole = role,
            Title = type,
            Message = type,
            Type = type,
            Link = $"/orders/{orderId}"
        };

    private sealed class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? RestaurantId => Guid.NewGuid();
        public bool IsSuperAdmin => false;
    }

    private sealed class TestCurrentUser(params string[] roles) : IAppCurrentUser
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public string? Email => "test@example.com";
        public bool IsAuthenticated => true;
        public IReadOnlyCollection<string> Roles { get; } = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .ToArray();

        public bool IsInRole(string role)
            => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
