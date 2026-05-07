using Market.Domain.Common.Enums;
using Market.Domain.Entities.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Create
{
    public sealed class CreateDraftCommandHandler(IAppDbContext db) : IRequestHandler<CreateDraftCommand>
    {
        public async Task Handle(CreateDraftCommand r, CancellationToken ct)
        {
            var domain = r.Domain.Trim().ToLowerInvariant();

            var domainProvisioned = await db.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Domain.ToLower() == domain, ct)
                || await db.Restaurants
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Domain.ToLower() == domain, ct);

            if (domainProvisioned)
                throw new MarketConflictException("Domain already in use.");

            var requestAlreadySubmitted = await db.TenantActivationRequests
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Domain.ToLower() == domain && x.Status != ActivationStatus.Activated, ct);

            if (requestAlreadySubmitted)
                return;

            var e = new TenantActivationRequest();
            e.EditDraft(
                r.RestaurantName, domain,
                r.OwnerFullName, r.OwnerEmail, r.OwnerPhone,
                r.Address, r.City, r.State
            );
            e.Submit();

            db.TenantActivationRequests.Add(e);
            await db.SaveChangesAsync(ct);
        }
    }
}
