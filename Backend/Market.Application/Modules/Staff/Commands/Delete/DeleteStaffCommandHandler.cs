using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Staff.Commands.Delete
{
    public sealed class DeleteStaffCommandHandler(
        IAppDbContext db,
        IStaffIdentityTerminationService staffIdentityTerminationService)
    : IRequestHandler<DeleteStaffCommand>
    {
        public async Task Handle(DeleteStaffCommand r, CancellationToken ct)
        {
            var e = await db.EmployeeProfiles
                .FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e is null) return; // idempotent

            await staffIdentityTerminationService.TerminateAsync(e.ApplicationUserId, ct);

            db.EmployeeProfiles.Remove(e);
            await db.SaveChangesAsync(ct);
        }
    }
}
