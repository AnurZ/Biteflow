namespace Market.Application.Abstractions;

public interface IPublicTenantResolver
{
    Task<PublicTenantContext> ResolveRequiredAsync(CancellationToken ct = default);
    Task<PublicTenantContext> ResolveRequiredAsync(Guid tenantId, Guid restaurantId, CancellationToken ct = default);
}
