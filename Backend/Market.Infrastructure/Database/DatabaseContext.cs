using Market.Application.Abstractions;
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
using Market.Shared.Constants;

namespace Market.Infrastructure.Database;

public partial class DatabaseContext : DbContext, IAppDbContext
{
    private sealed class SystemTenantContext : ITenantContext
    {
        public Guid? TenantId => SeedConstants.DefaultTenantId;
        public Guid? RestaurantId => null;
        public bool IsSuperAdmin => false;
    }

    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
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
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    private readonly TimeProvider _clock;
    private readonly ITenantContext _tenantContext;
    public Guid? CurrentTenantId => _tenantContext.TenantId;
    public bool IsSuperAdmin => _tenantContext.IsSuperAdmin;

    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock)
        : this(options, clock, new SystemTenantContext())
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock, ITenantContext tenantContext)
        : base(options)
    {
        _clock = clock;
        _tenantContext = tenantContext;
    }
}
