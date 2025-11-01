using Market.Domain.Entities.ActivationLinkEntity;
using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealIngredient;
using Market.Domain.Entities.Staff;
using Market.Domain.Entities.Tenants;

namespace Market.Application.Abstractions;

// Application layer
public interface IAppDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<ProductCategoryEntity> ProductCategories { get; }
    DbSet<AppUser> Users { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }

    DbSet<TenantActivationRequest> TenantActivationRequests { get; }
    DbSet<EmployeeProfile> EmployeeProfiles { get; }
    DbSet<ActivationLinkEntity> ActivationLinks { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Meal> Meals { get; }
    DbSet<MealIngredient> MealIngredients { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}