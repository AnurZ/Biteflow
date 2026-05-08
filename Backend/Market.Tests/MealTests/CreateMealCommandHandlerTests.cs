using Market.Application.Abstractions;
using Market.Application.Modules.Meal.Commands.Create;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.InventoryItem;
using Market.Infrastructure.Database;
using Market.Shared.Constants;
using Microsoft.Extensions.Time.Testing;

namespace Market.Tests.MealTests;

public sealed class CreateMealCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPersistMealAndIngredients_WithSingleSave()
    {
        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext(tenantId, restaurantId);
        await using var db = CreateContext(Guid.NewGuid().ToString(), tenantContext);

        var flour = new InventoryItem
        {
            Name = "Flour",
            Sku = "FLOUR-1",
            UnitType = UnitTypes.Gram,
            TenantId = tenantId,
            RestaurantId = restaurantId
        };
        var oil = new InventoryItem
        {
            Name = "Oil",
            Sku = "OIL-1",
            UnitType = UnitTypes.Milliliter,
            TenantId = tenantId,
            RestaurantId = restaurantId
        };

        db.InventoryItems.AddRange(flour, oil);
        await db.SaveChangesAsync();
        db.SaveChangesCallCount = 0;

        var handler = new CreateMealCommandHandler(db, tenantContext);
        var mealId = await handler.Handle(new CreateMealCommand
        {
            Name = "Flatbread",
            Description = "Simple test meal",
            BasePrice = 8.50m,
            IsAvailable = true,
            Ingredients =
            [
                new MealIngredientDto
                {
                    InventoryItemId = flour.Id,
                    Quantity = 200,
                    UnitType = UnitTypes.Gram
                },
                new MealIngredientDto
                {
                    InventoryItemId = oil.Id,
                    Quantity = 30,
                    UnitType = UnitTypes.Milliliter
                }
            ]
        }, CancellationToken.None);

        Assert.Equal(1, db.SaveChangesCallCount);

        var meal = await db.Meals
            .Include(x => x.Ingredients)
            .SingleAsync(x => x.Id == mealId);

        Assert.Equal("Flatbread", meal.Name);
        Assert.Equal(restaurantId, meal.RestaurantId);
        Assert.Equal(2, meal.Ingredients.Count);
        Assert.All(meal.Ingredients, ingredient => Assert.Equal(tenantId, ingredient.TenantId));
        Assert.Contains(meal.Ingredients, ingredient =>
            ingredient.InventoryItemId == flour.Id &&
            ingredient.Quantity == 200 &&
            ingredient.UnitTypes == UnitTypes.Gram);
        Assert.Contains(meal.Ingredients, ingredient =>
            ingredient.InventoryItemId == oil.Id &&
            ingredient.Quantity == 30 &&
            ingredient.UnitTypes == UnitTypes.Milliliter);
    }

    private static CountingDatabaseContext CreateContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new CountingDatabaseContext(options, new FakeTimeProvider(), tenantContext);
    }

    private sealed class CountingDatabaseContext(
        DbContextOptions<DatabaseContext> options,
        TimeProvider clock,
        ITenantContext tenantContext) : DatabaseContext(options, clock, tenantContext)
    {
        public int SaveChangesCallCount { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed class TestTenantContext(Guid tenantId, Guid restaurantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? RestaurantId => restaurantId;
        public bool IsSuperAdmin => false;
    }
}
