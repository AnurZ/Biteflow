using Market.Application.Abstractions;
using Market.Application.Modules.InventoryItem.Queries.GetById;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.InventoryItem;
using Market.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Tests.InventoryItemTests;

public sealed class GetInventoryItemByIdQueryHandlerTest
{
    [Fact]
    public async Task Handle_ShouldRejectInventoryItemFromOtherRestaurantInSameTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var restaurantA = Guid.NewGuid();
        var restaurantB = Guid.NewGuid();

        var tenantContext = new TestTenantContext(tenantId, restaurantB);

        await using var db = CreateContext(
            Guid.NewGuid().ToString(),
            tenantContext);

        var inventoryItem = new InventoryItem
        {
            Name = "Flour",
            Sku = "FLOUR-1",
            UnitType = UnitTypes.Gram,
            TenantId = tenantId,
            RestaurantId = restaurantA
        };

        db.InventoryItems.Add(inventoryItem);
        await db.SaveChangesAsync();

        var handler = new GetInventoryItemByIdQueryHandler(
            db,
            tenantContext);

        var query = new GetInventoryItemByIdQuery
        {
            Id = inventoryItem.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    private static DatabaseContext CreateContext(
        string dbName,
        ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new DatabaseContext(
            options,
            new FakeTimeProvider(),
            tenantContext);
    }

    private sealed class TestTenantContext(
        Guid tenantId,
        Guid restaurantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? RestaurantId => restaurantId;
        public bool IsSuperAdmin => false;
    }
}