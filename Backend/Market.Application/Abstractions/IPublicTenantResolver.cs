namespace Market.Application.Abstractions;

public interface IPublicTenantResolver
{
    Task<PublicTenantContext> ResolveRequiredAsync(CancellationToken ct = default);
}
