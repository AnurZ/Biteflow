using Market.Application.Abstractions;
using Market.Domain.Entities.ActivationLinkEntity;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealCategory;
using Market.Domain.Entities.MealIngredient;
using Market.Domain.Entities.Staff;
using Market.Domain.Entities.TableLayout;
using Market.Domain.Entities.TableReservations;
using Market.Domain.Entities.Tenants;

namespace Market.Infrastructure.Database;

public partial class DatabaseContext : DbContext, IAppDbContext
{
    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<TenantActivationRequest> TenantActivationRequests => Set<TenantActivationRequest>();

    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<ActivationLinkEntity> ActivationLinks => Set<ActivationLinkEntity>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealIngredient> MealIngredients => Set<MealIngredient>();
    public DbSet<MealCategory> MealCategories => Set<MealCategory>();
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<TableReservation> TableReservations => Set<TableReservation>();
    public DbSet<TableLayout> TableLayouts => Set<TableLayout>();

    private readonly TimeProvider _clock;
    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock) : base(options)
    {
        _clock = clock;
    }
}