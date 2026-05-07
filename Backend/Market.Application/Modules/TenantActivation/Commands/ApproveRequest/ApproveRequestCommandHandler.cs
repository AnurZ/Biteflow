using Market.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.ApproveRequest
{
    public sealed class ApproveRequestCommandHandler(
        IAppDbContext db,
        IActivationLinkService links,
        IEmailService emailService,
        ITenantContext _tenantContext)
    : IRequestHandler<ApproveRequestCommand, string>
    {
        public async Task<string> Handle(ApproveRequestCommand r, CancellationToken ct)
        {
            var tenantId = _tenantContext.RequireTenantId();

            var e = await db.TenantActivationRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == r.Id &&
                    x.TenantId == tenantId,
                    ct)
                ?? throw new MarketNotFoundException("Request not found");

            var link = await links.IssueLinkAsync(e.Id, ct);

            e.Approve(link);

            await db.SaveChangesAsync(ct);

            await SendApprovalEmailSafeAsync(
                emailService,
                e.OwnerEmail,
                e.OwnerFullName,
                e.RestaurantName,
                link,
                ct);

            return link;
        }

        private static async Task SendApprovalEmailSafeAsync(
            IEmailService emailService,
            string ownerEmail,
            string ownerFullName,
            string restaurantName,
            string activationLink,
            CancellationToken ct)
        {
            var subject = "Biteflow - Your Restaurant Has Been Approved";
            var body = $"""
Hello {ownerFullName},

Your restaurant request for "{restaurantName}" has been approved.

To continue onboarding, please activate your account using this link:
{activationLink}

After activation, we will send your admin username and a one-time link to set your password.

Best regards,
Biteflow Team
""";

            try
            {
                await emailService.SendAsync(ownerEmail, subject, body, ct);
            }
            catch
            {
                // Email sending is best-effort and must not block approval.
            }
        }
    }

}
