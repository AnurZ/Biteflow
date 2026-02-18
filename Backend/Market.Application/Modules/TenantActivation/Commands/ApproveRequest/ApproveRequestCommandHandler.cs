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
        IEmailService emailService)
    : IRequestHandler<ApproveRequestCommand, string>
    {
        public async Task<string> Handle(ApproveRequestCommand r, CancellationToken ct)
        {
            var e = await db.TenantActivationRequests.FindAsync([r.Id], ct)
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

After activation, we will send your first-time access username and password.

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
