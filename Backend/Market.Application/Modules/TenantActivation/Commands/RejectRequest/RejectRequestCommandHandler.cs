using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.RejectRequest
{
    public sealed class RejectRequestCommandHandler(IAppDbContext db)
        : IRequestHandler<RejectRequestCommand>
    {
        public async Task Handle(RejectRequestCommand r, CancellationToken ct)
        {

            var reason = r.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                throw new ValidationException("Reason is required.");

            // Load the request
            var e = await db.TenantActivationRequests.FindAsync(new object[] { r.Id }, ct)
                    ?? throw new MarketNotFoundException("Request not found");

            e.Reject(reason);

            // revoke any active links for this request
            var activeLinks = await db.ActivationLinks
                .Where(x => x.RequestId == r.Id &&
                            x.ConsumedAtUtc == null &&
                            x.ExpiresAtUtc > DateTimeOffset.UtcNow)
                .ToListAsync(ct);

            if (activeLinks.Count > 0)
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var link in activeLinks)
                    link.ExpiresAtUtc = now; 
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
