using Market.Application.Common.Exceptions;

namespace Market.Application.Abstractions;

public static class TenantContextExtensions
{
    public static Guid RequireTenantId(this ITenantContext tenantContext)
    {
        var tenantId = tenantContext.TenantId;
        if (tenantId is null || tenantId == Guid.Empty)
        {
            throw new TenantContextMissingException();
        }

        return tenantId.Value;
    }

    public static Guid RequireRestaurantId(this ITenantContext tenantContext)
    {
        var restaurantId = tenantContext.RestaurantId;
        if (restaurantId is null || restaurantId == Guid.Empty)
        {
            throw new TenantContextMissingException("Restaurant context is missing or invalid.");
        }

        return restaurantId.Value;
    }
}
