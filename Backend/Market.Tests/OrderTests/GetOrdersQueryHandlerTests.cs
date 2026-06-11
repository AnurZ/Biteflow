using Market.Application.Abstractions;
using Market.Application.Common;
using Market.Application.Modules.Orders.Queries.GetOrders;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Orders;
using Microsoft.Extensions.Time.Testing;

namespace Market.Tests.OrderTests;

public sealed class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRequestedPageWithTotalAndNewestFirstOrdering()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var oldOrder = await SeedOrderAsync(db, tenantId, OrderStatus.New, new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var middleOrder = await SeedOrderAsync(db, tenantId, OrderStatus.New, new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc));
        var newestOrder = await SeedOrderAsync(db, tenantId, OrderStatus.New, new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var handler = new GetOrdersQueryHandler(db);

        var result = await handler.Handle(new GetOrdersQuery
        {
            Paging = new PageRequest { Page = 1, PageSize = 2 }
        }, CancellationToken.None);

        Assert.Equal(3, result.Total);
        Assert.Equal([newestOrder.Id, middleOrder.Id], result.Items.Select(x => x.Id).ToArray());
        Assert.DoesNotContain(result.Items, x => x.Id == oldOrder.Id);
    }

    [Fact]
    public async Task Handle_ShouldCombineStatusFilteringWithPagination()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        await SeedOrderAsync(db, tenantId, OrderStatus.Cancelled, new DateTime(2026, 1, 1, 13, 0, 0, DateTimeKind.Utc));
        var newestActiveOrder = await SeedOrderAsync(db, tenantId, OrderStatus.Cooking, new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var secondActiveOrder = await SeedOrderAsync(db, tenantId, OrderStatus.New, new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc));
        var thirdActiveOrder = await SeedOrderAsync(db, tenantId, OrderStatus.New, new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var handler = new GetOrdersQueryHandler(db);

        var result = await handler.Handle(new GetOrdersQuery
        {
            Statuses = [OrderStatus.New, OrderStatus.Cooking],
            Paging = new PageRequest { Page = 2, PageSize = 2 }
        }, CancellationToken.None);

        Assert.Equal(3, result.Total);
        var item = Assert.Single(result.Items);
        Assert.Equal(thirdActiveOrder.Id, item.Id);
        Assert.DoesNotContain(result.Items, x => x.Id == newestActiveOrder.Id || x.Id == secondActiveOrder.Id);
    }

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
        DateTime createdAtUtc)
    {
        var order = new Order
        {
            TenantId = tenantId,
            Status = status,
            TableNumber = 5,
            Items =
            [
                new OrderItem
                {
                    TenantId = tenantId,
                    Name = $"Item {Guid.NewGuid():N}",
                    Quantity = 1,
                    UnitPrice = 10m
                }
            ]
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        order.CreatedAtUtc = createdAtUtc;
        await db.SaveChangesAsync();

        return order;
    }

    private sealed class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? RestaurantId => Guid.NewGuid();
        public bool IsSuperAdmin => false;
    }
}
