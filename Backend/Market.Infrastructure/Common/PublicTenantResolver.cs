using FluentValidation;
using Market.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Market.Infrastructure.Common;

public sealed class PublicTenantResolver(IHttpContextAccessor accessor, IAppDbContext db) : IPublicTenantResolver
{
    public async Task<PublicTenantContext> ResolveRequiredAsync(CancellationToken ct = default)
    {
        var domain = ResolveDomain();
        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ValidationException("Tenant domain is required.");
        }

        var context = await db.Restaurants
            .AsNoTracking()
            // Public domain resolution runs before an authenticated tenant context exists.
            .IgnoreQueryFilters()
            .Where(r => r.IsActive && r.Domain.ToLower() == domain)
            .Join(
                db.Tenants.AsNoTracking()
                    // Public domain resolution must see active tenants across all tenant scopes.
                    .IgnoreQueryFilters()
                    .Where(t => t.IsActive),
                r => r.TenantId,
                t => t.Id,
                (r, t) => new PublicTenantContext(t.Id, r.Id, r.Domain))
            .FirstOrDefaultAsync(ct);

        if (context is null)
        {
            throw new ValidationException("Tenant domain is invalid or inactive.");
        }

        return context;
    }

    public async Task<PublicTenantContext> ResolveRequiredAsync(Guid tenantId, Guid restaurantId, CancellationToken ct = default)
    {
        if (tenantId == Guid.Empty || restaurantId == Guid.Empty)
        {
            throw new ValidationException("Tenant and restaurant ids are required.");
        }

        var context = await db.Restaurants
            .AsNoTracking()
            // Explicit public resolution validates tenant/restaurant ids before authentication.
            .IgnoreQueryFilters()
            .Where(r => r.IsActive && r.Id == restaurantId && r.TenantId == tenantId)
            .Join(
                db.Tenants.AsNoTracking()
                    // Explicit public resolution must see active tenants across all tenant scopes.
                    .IgnoreQueryFilters()
                    .Where(t => t.IsActive && t.Id == tenantId),
                r => r.TenantId,
                t => t.Id,
                (r, t) => new PublicTenantContext(t.Id, r.Id, r.Domain))
            .FirstOrDefaultAsync(ct);

        if (context is null)
        {
            throw new ValidationException("Tenant or restaurant is invalid or inactive.");
        }

        return context;
    }

    private string? ResolveDomain()
    {
        var request = accessor.HttpContext?.Request;
        if (request is null)
        {
            return null;
        }

        var headerDomain = request.Headers["X-Tenant-Domain"].FirstOrDefault();
        var raw = string.IsNullOrWhiteSpace(headerDomain)
            ? request.Host.Host
            : headerDomain;

        raw = raw?.Trim().TrimEnd('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return raw.StartsWith("www.", StringComparison.Ordinal)
            ? raw[4..]
            : raw;
    }
}
