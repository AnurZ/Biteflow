using Market.Application.Modules.Staff.Commands.Delete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Commands.Delete
{
    public sealed class DeleteInventoryItemCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteInventoryItemCommand>
    {
        public async Task Handle(DeleteInventoryItemCommand r, CancellationToken ct)
        {
            var ie = await db.InventoryItems.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (ie is null) return; // idempotent
            db.InventoryItems.Remove(ie);
            await db.SaveChangesAsync(ct);
        }
    }
}
