namespace Market.Application.Abstractions;

public sealed record PublicTenantContext(Guid TenantId, Guid RestaurantId, string Domain);
