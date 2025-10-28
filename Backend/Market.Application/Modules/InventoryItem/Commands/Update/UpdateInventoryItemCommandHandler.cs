using Market.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.InventoryItem.Commands.Update
{
    public sealed class UpdateInventoryItemCommandHandler(IAppDbContext db)
        : IRequestHandler<UpdateInventoryItemCommand>
    {
        public async Task Handle(UpdateInventoryItemCommand r, CancellationToken ct)
        {
            var ie = await db.InventoryItems.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (ie is null)
                throw new KeyNotFoundException("Inventory item not found.");

            bool duplicateExists = await db.InventoryItems
                .AnyAsync(x => x.Id != r.Id &&
                               x.RestaurantId == r.RestaurantId &&
                               x.Name.ToLower() == r.Name.ToLower(), ct);

            if (duplicateExists)
                throw new ValidationException($"An item with the name '{r.Name}' already exists for this restaurant.");

            ie.Name = r.Name.Trim();
            ie.Sku = r.Sku.Trim();
            ie.RestaurantId = r.RestaurantId;
            ie.UnitType = r.UnitType;
            ie.ReorderQty = r.ReorderQty;
            ie.ReorderFrequency = r.ReorderFrequency;
            ie.CurrentQty = r.CurrentQty;

            await db.SaveChangesAsync(ct);
        }
    }
}
