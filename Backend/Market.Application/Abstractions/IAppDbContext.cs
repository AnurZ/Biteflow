using Market.Domain.Entities.ActivationLinkEntity;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealCategory;
using Market.Domain.Entities.MealIngredient;
using Market.Domain.Entities.Notifications;
using Market.Domain.Entities.Orders;
using Market.Domain.Entities.Staff;
using Market.Domain.Entities.TableLayout;
using Market.Domain.Entities.TableReservations;
using Market.Domain.Entities.Tenants;

namespace Market.Application.Abstractions;

// Application layer
public interface IAppDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<ProductCategoryEntity> ProductCategories { get; }
    DbSet<AppUser> Users { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }

    DbSet<Tenant> Tenants { get; }
    DbSet<Restaurant> Restaurants { get; }
    DbSet<TenantActivationRequest> TenantActivationRequests { get; }
    DbSet<EmployeeProfile> EmployeeProfiles { get; }
    DbSet<ActivationLinkEntity> ActivationLinks { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Meal> Meals { get; }
    DbSet<MealIngredient> MealIngredients { get; }
    DbSet<MealCategory> MealCategories { get; }
    DbSet<DiningTable> DiningTables { get; }
    DbSet<TableReservation> TableReservations { get; }
    DbSet<TableLayout> TableLayouts { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<NotificationEntity> Notifications { get; }


    Task<int> SaveChangesAsync(CancellationToken ct);
}
