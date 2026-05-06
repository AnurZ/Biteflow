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
            .IgnoreQueryFilters()
            .Where(r => r.IsActive && r.Domain.ToLower() == domain)
            .Join(
                db.Tenants.AsNoTracking().IgnoreQueryFilters().Where(t => t.IsActive),
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
