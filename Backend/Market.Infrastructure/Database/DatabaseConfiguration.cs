using Market.Domain.Common;
using Market.Application.Common.Exceptions;
using Market.Domain.Entities.Tenants;
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
                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        if (entry.Entity is TenantActivationRequest)
                        {
                            // Activation requests are created before a tenant exists.
                        }
                        else if (CurrentTenantId.HasValue && !IsSuperAdmin)
                        {
                            entry.Entity.TenantId = CurrentTenantId.Value;
                        }
                        else
                        {
                            throw new TenantContextMissingException(
                                $"Cannot create {entry.Entity.GetType().Name} without an explicit tenant.");
                        }
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

        ApplyGlobalFilters(modelBuilder);

        StaticDataSeeder.Seed(modelBuilder); // static data
    }

    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                            .HasQueryFilter(CreateGlobalFilter(entityType.ClrType));
            }
        }
    }

    private LambdaExpression CreateGlobalFilter(Type entityClrType)
    {
        Expression<Func<BaseEntity, bool>> baseFilter = e =>
            !EF.Property<bool>(e, nameof(BaseEntity.IsDeleted)) &&
            (IsSuperAdmin || EF.Property<Guid>(e, nameof(BaseEntity.TenantId)) == CurrentTenantId);

        var parameter = Expression.Parameter(entityClrType, "e");
        var body = new ReplaceParameterVisitor(baseFilter.Parameters[0], parameter)
            .Visit(baseFilter.Body)!;

        return Expression.Lambda(body, parameter);
    }

    private sealed class ReplaceParameterVisitor(
        ParameterExpression source,
        ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
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
