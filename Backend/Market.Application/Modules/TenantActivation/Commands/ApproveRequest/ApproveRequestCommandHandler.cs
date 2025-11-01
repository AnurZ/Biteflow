using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.ApproveRequest
{
    public sealed class ApproveRequestCommandHandler(IAppDbContext db, IActivationLinkService links)
    : IRequestHandler<ApproveRequestCommand, string>
    {
        public async Task<string> Handle(ApproveRequestCommand r, CancellationToken ct)
        {
            var e = await db.TenantActivationRequests.FindAsync([r.Id], ct)
                    ?? throw new MarketNotFoundException("Request not found");
            var link = await links.IssueLinkAsync(e.Id, ct);
            e.Approve(link);
            await db.SaveChangesAsync(ct);
            return link;
        }
    }

}
