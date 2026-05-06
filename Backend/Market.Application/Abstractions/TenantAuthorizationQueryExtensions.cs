using Market.Domain.Common;

namespace Market.Application.Abstractions;

public static class TenantAuthorizationQueryExtensions
{
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
        if (tenantContext.IsSuperAdmin)
        {
            return query;
        }

        var tenantId = tenantContext.RequireTenantId();
        var restaurantId = tenantContext.RequireRestaurantId();

        return query.Where(x =>
            x.TenantId == tenantId &&
            EF.Property<Guid>(x, "RestaurantId") == restaurantId);
    }

    public static IQueryable<TEntity> WhereNullableRestaurantOwned<TEntity>(
        this IQueryable<TEntity> query,
        ITenantContext tenantContext)
        where TEntity : BaseEntity
    {
        if (tenantContext.IsSuperAdmin)
        {
            return query;
        }

        var tenantId = tenantContext.RequireTenantId();
        var restaurantId = tenantContext.RequireRestaurantId();

        return query.Where(x =>
            x.TenantId == tenantId &&
            EF.Property<Guid?>(x, "RestaurantId") == restaurantId);
    }
}
