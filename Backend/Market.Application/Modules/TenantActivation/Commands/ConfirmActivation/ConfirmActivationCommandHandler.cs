using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.ConfirmActivation
{
    public sealed class ConfirmActivationHandler(IAppDbContext db, IActivationLinkService links)
    : IRequestHandler<ConfirmActivationCommand, Guid>
    {
        public async Task<Guid> Handle(ConfirmActivationCommand r, CancellationToken ct)
        {
            var requestId = await links.ValidateAndConsumeAsync(r.token, ct);

            var e = await db.TenantActivationRequests.FindAsync(new object[] { requestId }, ct)
                     ?? throw new MarketNotFoundException("Request not found");

            var tenantId = Guid.NewGuid();

            e.MarkActivated(tenantId);
            await db.SaveChangesAsync(ct);

            return tenantId;
        }
    }
}
