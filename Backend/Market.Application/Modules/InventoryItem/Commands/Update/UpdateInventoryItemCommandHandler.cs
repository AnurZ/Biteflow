using Market.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.InventoryItem.Commands.Update
{
    public sealed class UpdateInventoryItemCommandHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
        : IRequestHandler<UpdateInventoryItemCommand>
    {
        public async Task Handle(UpdateInventoryItemCommand r, CancellationToken ct)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var item = await db.InventoryItems
                .FirstOrDefaultAsync(x =>
                    x.Id == r.Id &&
                    x.RestaurantId == restaurantId,
                    ct);

            if (item is null)
                throw new KeyNotFoundException("Inventory item not found.");

            if (string.IsNullOrWhiteSpace(r.Name))
                throw new ValidationException("Name is required.");

            if (string.IsNullOrWhiteSpace(r.Sku))
                throw new ValidationException("SKU is required.");

            var nameExists = await db.InventoryItems
                .AnyAsync(x =>
                    x.Id != r.Id &&
                    x.RestaurantId == restaurantId &&
                    x.Name.ToLower() == r.Name.Trim().ToLower(),
                    ct);

            if (nameExists)
                throw new ValidationException($"An item with the name '{r.Name}' already exists.");

            var skuExists = await db.InventoryItems
                .AnyAsync(x =>
                    x.Id != r.Id &&
                    x.RestaurantId == restaurantId &&
                    x.Sku == r.Sku,
                    ct);

            if (skuExists)
                throw new ValidationException($"SKU '{r.Sku}' already exists in this restaurant.");

            item.Name = r.Name.Trim();
            item.Sku = r.Sku; 
            item.UnitType = r.UnitType;
            item.ReorderQty = r.ReorderQty;
            item.ReorderFrequency = r.ReorderFrequency;
            item.CurrentQty = r.CurrentQty;

            await db.SaveChangesAsync(ct);
        }
    }
}