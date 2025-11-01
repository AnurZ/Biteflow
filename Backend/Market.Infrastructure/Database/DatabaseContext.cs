using Market.Application.Abstractions;
using Market.Domain.Entities.ActivationLinkEntity;
using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealIngredient;
using Market.Domain.Entities.Staff;
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

    private readonly TimeProvider _clock;
    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock) : base(options)
    {
        _clock = clock;
    }
}