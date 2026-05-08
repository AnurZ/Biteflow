using Market.Domain.Common;

namespace Market.Application.Abstractions;

public static class TenantAuthorizationQueryExtensions
{
    [Obsolete("Tenant isolation is enforced by the DatabaseContext global query filter. Do not add explicit tenant filters in handlers.")]
    public static IQueryable<TEntity> WhereTenantOwned<TEntity>(
        this IQueryable<TEntity> query,
        ITenantContext tenantContext)
        where TEntity : BaseEntity
    {
        if (tenantContext.IsSuperAdmin)
        {
            return query;
        }

        var tenantId = tenantContext.RequireTenantId();
        return query.Where(x => x.TenantId == tenantId);
    }

    public static IQueryable<TEntity> WhereRestaurantOwned<TEntity>(
        this IQueryable<TEntity> query,
        ITenantContext tenantContext)
        where TEntity : BaseEntity
    {
        return query.WhereCurrentRestaurant(tenantContext);
    }

    public static IQueryable<TEntity> WhereNullableRestaurantOwned<TEntity>(
        this IQueryable<TEntity> query,
        ITenantContext tenantContext)
        where TEntity : BaseEntity
    {
        return query.WhereCurrentRestaurant(tenantContext);
    }

    public static IQueryable<TEntity> WhereCurrentRestaurant<TEntity>(
        this IQueryable<TEntity> query,
        ITenantContext tenantContext)
        where TEntity : BaseEntity
    {
        if (tenantContext.IsSuperAdmin)
        {
            return query;
        }

        var restaurantId = tenantContext.RequireRestaurantId();

        return query.Where(x => EF.Property<Guid?>(x, "RestaurantId") == restaurantId);
    }
}
