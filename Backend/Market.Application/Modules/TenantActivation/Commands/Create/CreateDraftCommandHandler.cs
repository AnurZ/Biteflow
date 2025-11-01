using Market.Domain.Entities.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Create
{
    public sealed class CreateDraftCommandHandler(IAppDbContext db) : IRequestHandler<CreateDraftCommand, int>
    {
        public async Task<int> Handle(CreateDraftCommand r, CancellationToken ct)
        {
            var exists = await db.TenantActivationRequests.AnyAsync(x => x.Domain == r.Domain, ct);
            if (exists) throw new ValidationException("Domain already in use.");

            var e = new TenantActivationRequest();
            e.EditDraft(r.RestaurantName, r.Domain, r.OwnerFullName, r.OwnerEmail, r.OwnerPhone, r.Address, r.City, r.State);

            db.TenantActivationRequests.Add(e);
            await db.SaveChangesAsync(ct);
            return e.Id;
        }
    }
}
