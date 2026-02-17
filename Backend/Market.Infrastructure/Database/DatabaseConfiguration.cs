using Market.Domain.Common;
using Market.Infrastructure.Database.Seeders;
using System.Linq.Expressions;

namespace Market.Infrastructure.Database;

public partial class DatabaseContext
{
    private DateTime UtcNow => _clock.GetUtcNow().UtcDateTime;

    private void ApplyAuditAndSoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.TenantId == Guid.Empty &&
                        CurrentTenantId.HasValue &&
                        !IsSuperAdmin)
                    {
                        entry.Entity.TenantId = CurrentTenantId.Value;
                    }

                    entry.Entity.CreatedAtUtc = UtcNow;
                    entry.Entity.ModifiedAtUtc = null; // ili = UtcNow
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAtUtc = UtcNow;
                    break;

                case EntityState.Deleted:
                    // soft-delete: set is Modified and IsDeleted
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.ModifiedAtUtc = UtcNow;
                    break;
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<decimal?>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);

        ApplyGlobalFielters(modelBuilder);

        StaticDataSeeder.Seed(modelBuilder); // static data
    }

    private void ApplyGlobalFielters(ModelBuilder modelBuilder)
    {
        // Apply a global filter to all entities inheriting from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeleted = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var notDeleted = Expression.Equal(isDeleted, Expression.Constant(false));

                var context = Expression.Constant(this);
                var isSuperAdmin = Expression.Property(context, nameof(IsSuperAdmin));
                var currentTenantId = Expression.Property(context, nameof(CurrentTenantId));

                var tenantProp = Expression.Property(parameter, nameof(BaseEntity.TenantId));
                var tenantPropNullable = Expression.Convert(tenantProp, typeof(Guid?));
                var tenantMatch = Expression.Equal(tenantPropNullable, currentTenantId);
                var tenantScope = Expression.OrElse(isSuperAdmin, tenantMatch);

                var body = Expression.AndAlso(notDeleted, tenantScope);
                var lambda = Expression.Lambda(body, parameter);

                modelBuilder.Entity(entityType.ClrType)
                            .HasQueryFilter(lambda);
            }
        }
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        ApplyAuditAndSoftDelete();

        return base.SaveChangesAsync(cancellationToken);
    }
}
