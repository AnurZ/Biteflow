namespace Market.Application.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? RestaurantId { get; }
    bool IsSuperAdmin { get; }
}
