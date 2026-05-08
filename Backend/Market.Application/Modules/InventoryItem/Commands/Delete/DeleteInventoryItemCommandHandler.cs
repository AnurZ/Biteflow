using Market.Application.Modules.Staff.Commands.Delete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Commands.Delete
{
    public sealed class DeleteInventoryItemCommandHandler(IAppDbContext db, ITenantContext tenantContext)
    : IRequestHandler<DeleteInventoryItemCommand>
    {
        public async Task Handle(DeleteInventoryItemCommand r, CancellationToken ct)
        {
            var ie = await db.InventoryItems
                .WhereCurrentRestaurant(tenantContext)
                .FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (ie is null)
                throw new KeyNotFoundException("Inventory item not found.");

            db.InventoryItems.Remove(ie);
            await db.SaveChangesAsync(ct);
        }
    }
}
