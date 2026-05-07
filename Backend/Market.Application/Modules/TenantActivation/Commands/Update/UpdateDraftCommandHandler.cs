using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Update
{
    public sealed class UpdateDraftCommandHandler(IAppDbContext db) : IRequestHandler<UpdateDraftCommand>
    {
        public async Task Handle(UpdateDraftCommand r, CancellationToken ct)
        {
            var e = await db.TenantActivationRequests
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == r.Id, ct)
                ?? throw new MarketNotFoundException("Request not found");

            // check for domain uniqueness
            if (!string.Equals(e.Domain, r.Domain, StringComparison.OrdinalIgnoreCase))
            {
                var domain = r.Domain.Trim().ToLowerInvariant();
                var exists = await db.TenantActivationRequests
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Id != r.Id && x.Domain.ToLower() == domain, ct);
                if (exists) throw new ValidationException("Domain already in use.");
            }
            e.EditDraft(r.RestaurantName, r.Domain, r.OwnerFullName, r.OwnerEmail, r.OwnerPhone,
                        r.Address, r.City, r.State);
            await db.SaveChangesAsync(ct);
        }
    }
}
