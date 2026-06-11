using System.ComponentModel.DataAnnotations;
using Market.Application.Abstractions;
using Market.Application.Modules.Orders.Commands.CreateOrder;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.TableLayout;
using Market.Shared.Constants;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Market.Tests.OrderTests;

public sealed class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateOrderWithResolvedDiningTableNumber()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId, number: 7);
        var meal = await SeedMealAsync(db, tenantId, restaurantId, name: "Burger", basePrice: 12.50m);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        var result = await handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 2 }]
        }, CancellationToken.None);
        var orderId = result.Id;

        var order = await db.Orders
            .Include(x => x.Items)
            .SingleAsync(x => x.Id == orderId);

        Assert.Equal(table.Id, order.DiningTableId);
        Assert.Equal(7, order.TableNumber);
        var item = Assert.Single(order.Items);
        Assert.Equal(meal.Id, item.MealId);
        Assert.Equal("Burger", item.Name);
        Assert.Equal(12.50m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);

        var notification = await db.Notifications.SingleAsync(x => x.OrderId == orderId);
        Assert.Equal(RoleNames.Kitchen, notification.TargetRole);
        Assert.Equal("OrderCreated", notification.Type);
        Assert.Equal($"/kitchen/orders/{orderId}", notification.Link);
        Assert.Equal(notification.Id, result.CreatedNotification.Id);
        Assert.Equal(RoleNames.Kitchen, result.CreatedNotification.TargetRole);
        Assert.Equal("OrderCreated", result.CreatedNotification.Type);
        Assert.Equal($"/kitchen/orders/{orderId}", result.CreatedNotification.Link);
    }

    [Fact]
    public async Task Handle_ShouldRejectMissingDiningTableId()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var meal = await SeedMealAsync(db, tenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateOrderCommand
        {
            TableNumber = 7,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldRejectInvalidDiningTableId()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var meal = await SeedMealAsync(db, tenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = 999,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldRejectInactiveDiningTableId()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId, isActive: false);
        var meal = await SeedMealAsync(db, tenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldRejectOtherTenantDiningTableId()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var otherTable = await SeedTableAsync(db, otherTenantId, restaurantId);
        var meal = await SeedMealAsync(db, tenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = otherTable.Id,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldRejectOtherRestaurantDiningTableId()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var otherRestaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var otherTable = await SeedTableAsync(db, tenantId, otherRestaurantId);
        var meal = await SeedMealAsync(db, tenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = otherTable.Id,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldIgnoreConflictingTableNumber()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId, number: 12);
        var meal = await SeedMealAsync(db, tenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        var result = await handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            TableNumber = 999,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None);
        var orderId = result.Id;

        var order = await db.Orders.SingleAsync(x => x.Id == orderId);
        Assert.Equal(12, order.TableNumber);
    }

    [Fact]
    public async Task Handle_ShouldIgnoreClientNameAndUnitPriceForMenuItems()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId);
        var meal = await SeedMealAsync(db, tenantId, restaurantId, name: "Ribeye", basePrice: 29.99m);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        var result = await handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items =
            [
                new CreateOrderItemDto
                {
                    MealId = meal.Id,
                    Name = "Attacker Special",
                    UnitPrice = 0.01m,
                    Quantity = 1
                }
            ]
        }, CancellationToken.None);
        var orderId = result.Id;

        var item = await db.OrderItems.SingleAsync(x => x.OrderId == orderId);
        Assert.Equal("Ribeye", item.Name);
        Assert.Equal(29.99m, item.UnitPrice);
    }

    [Fact]
    public async Task Handle_ShouldRejectUnavailableMeal()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId);
        var meal = await SeedMealAsync(db, tenantId, restaurantId, isAvailable: false);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldRejectOtherTenantMeal()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId);
        var meal = await SeedMealAsync(db, otherTenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldRejectOtherRestaurantMeal()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var otherRestaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId);
        var meal = await SeedMealAsync(db, tenantId, otherRestaurantId);
        var handler = CreateHandler(db, tenantContext, RoleNames.Waiter);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items = [new CreateOrderItemDto { MealId = meal.Id, Quantity = 1 }]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Theory]
    [InlineData(RoleNames.Waiter)]
    [InlineData(RoleNames.Admin)]
    public async Task Handle_ShouldAllowCustomItemsForAllowedRoles(string role)
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId);
        var handler = CreateHandler(db, tenantContext, role);

        var result = await handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items =
            [
                new CreateOrderItemDto
                {
                    IsCustom = true,
                    Name = "Off-menu soup",
                    Quantity = 1,
                    UnitPrice = 4.75m
                }
            ]
        }, CancellationToken.None);
        var orderId = result.Id;

        var item = await db.OrderItems.SingleAsync(x => x.OrderId == orderId);
        Assert.Null(item.MealId);
        Assert.Equal("Off-menu soup", item.Name);
        Assert.Equal(4.75m, item.UnitPrice);
    }

    [Theory]
    [InlineData(RoleNames.Kitchen)]
    [InlineData(RoleNames.Staff)]
    [InlineData("")]
    public async Task Handle_ShouldRejectCustomItemsForDisallowedRoles(string role)
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);
        var table = await SeedTableAsync(db, tenantId, restaurantId);
        var handler = string.IsNullOrWhiteSpace(role)
            ? CreateHandler(db, tenantContext)
            : CreateHandler(db, tenantContext, role);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new CreateOrderCommand
        {
            DiningTableId = table.Id,
            Items =
            [
                new CreateOrderItemDto
                {
                    IsCustom = true,
                    Name = "Off-menu soup",
                    Quantity = 1,
                    UnitPrice = 4.75m
                }
            ]
        }, CancellationToken.None));

        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public void Validator_ShouldRequireDiningTableId()
    {
        var result = new CreateOrderCommandValidator().Validate(new CreateOrderCommand
        {
            TableNumber = 7,
            Items = [new CreateOrderItemDto { MealId = 1, Quantity = 1 }]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateOrderCommand.DiningTableId));
    }

    [Fact]
    public void Validator_ShouldRequireMealIdForMenuItems()
    {
        var result = new CreateOrderCommandValidator().Validate(new CreateOrderCommand
        {
            DiningTableId = 1,
            Items = [new CreateOrderItemDto { Quantity = 1 }]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("MealId"));
    }

    [Fact]
    public void Validator_ShouldRejectAmbiguousCustomItem()
    {
        var result = new CreateOrderCommandValidator().Validate(new CreateOrderCommand
        {
            DiningTableId = 1,
            Items =
            [
                new CreateOrderItemDto
                {
                    IsCustom = true,
                    MealId = 1,
                    Name = "Custom",
                    UnitPrice = 1,
                    Quantity = 1
                }
            ]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Custom order items cannot reference a meal"));
    }

    [Fact]
    public void Validator_ShouldAllowClientNameAndUnitPriceOnMenuItemsForBackwardCompatibility()
    {
        var result = new CreateOrderCommandValidator().Validate(new CreateOrderCommand
        {
            DiningTableId = 1,
            Items =
            [
                new CreateOrderItemDto
                {
                    MealId = 1,
                    Name = "Ignored",
                    UnitPrice = 0.01m,
                    Quantity = 1
                }
            ]
        });

        Assert.True(result.IsValid);
    }

    private static CreateOrderCommandHandler CreateHandler(
        IAppDbContext db,
        ITenantContext tenantContext,
        params string[] roles)
        => new(
            db,
            tenantContext,
            new TestCurrentUser(roles),
            NullLogger<CreateOrderCommandHandler>.Instance);

    private static DatabaseContext CreateContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new DatabaseContext(options, new FakeTimeProvider(), tenantContext);
    }

    private static async Task<DiningTable> SeedTableAsync(
        DatabaseContext db,
        Guid tenantId,
        Guid restaurantId,
        int number = 5,
        bool isActive = true)
    {
        var layout = new TableLayout
        {
            Name = $"Layout {Guid.NewGuid():N}",
            TenantId = tenantId,
            RestaurantId = restaurantId
        };

        db.TableLayouts.Add(layout);
        await db.SaveChangesAsync();

        var table = new DiningTable
        {
            Number = number,
            NumberOfSeats = 4,
            IsActive = isActive,
            TableLayoutId = layout.Id,
            X = 0,
            Y = 0,
            Width = 80,
            Height = 80,
            Shape = "rectangle",
            Color = "#ffffff",
            TableType = TableTypes.LowTable,
            Status = TableStatus.Free,
            TenantId = tenantId
        };

        db.DiningTables.Add(table);
        await db.SaveChangesAsync();

        return table;
    }

    private static async Task<Meal> SeedMealAsync(
        DatabaseContext db,
        Guid tenantId,
        Guid restaurantId,
        string name = "Pasta",
        decimal basePrice = 10.25m,
        bool isAvailable = true)
    {
        var meal = new Meal
        {
            Name = name,
            Description = "Test meal",
            BasePrice = basePrice,
            IsAvailable = isAvailable,
            RestaurantId = restaurantId,
            TenantId = tenantId
        };

        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        return meal;
    }

    private sealed class TestTenantContext(Guid tenantId, Guid restaurantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? RestaurantId => restaurantId;
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
