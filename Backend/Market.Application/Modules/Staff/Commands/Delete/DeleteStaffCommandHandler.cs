using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Staff.Commands.Delete
{
    public sealed class DeleteStaffCommandHandler(IAppDbContext db, ITenantContext tenantContext)
    : IRequestHandler<DeleteStaffCommand>
    {
        public async Task Handle(DeleteStaffCommand r, CancellationToken ct)
        {
            var e = await db.EmployeeProfiles
                .WhereTenantOwned(tenantContext)
                .FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e is null) return; // idempotent
            db.EmployeeProfiles.Remove(e);
            await db.SaveChangesAsync(ct);
        }
    }
}
