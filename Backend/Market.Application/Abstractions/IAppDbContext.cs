using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Staff;

namespace Market.Application.Abstractions;

// Application layer
public interface IAppDbContext
{
    DbSet<ProductEntity> Products { get; }
    DbSet<ProductCategoryEntity> ProductCategories { get; }
    DbSet<AppUser> Users { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }

    DbSet<EmployeeProfile> EmployeeProfiles { get; }
    DbSet<InventoryItem> InventoryItems { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}