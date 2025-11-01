using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Submit
{
    public sealed class SubmitDraftCommandHandler(IAppDbContext db) : IRequestHandler<SubmitDraftCommand>
    {
        public async Task Handle(SubmitDraftCommand r, CancellationToken ct)
        {
            var e = await db.TenantActivationRequests.FindAsync([r.Id], ct)
               ?? throw new MarketNotFoundException("Request not found");
            e.Submit();
            await db.SaveChangesAsync(ct);
        }
    }
}
