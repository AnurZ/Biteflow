using Market.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.InventoryItem.Commands.Create
{
    public sealed class CreateInventoryItemCommandHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
        : IRequestHandler<CreateInventoryItemCommand, int>
    {
        public async Task<int> Handle(CreateInventoryItemCommand r, CancellationToken ct)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            if (string.IsNullOrWhiteSpace(r.Name))
                throw new ValidationException("Name is required.");

            if (string.IsNullOrWhiteSpace(r.Sku))
                throw new ValidationException("SKU is required.");

            var skuExists = await db.InventoryItems
                .AnyAsync(i =>
                    i.RestaurantId == restaurantId &&
                    i.Sku == r.Sku,
                    ct);

            if (skuExists)
                throw new ValidationException($"SKU '{r.Sku}' already exists in this restaurant.");

            var nameExists = await db.InventoryItems
                .AnyAsync(i =>
                    i.RestaurantId == restaurantId &&
                    i.Name.ToLower() == r.Name.Trim().ToLower(),
                    ct);

            if (nameExists)
                throw new ValidationException($"An item with the name '{r.Name}' already exists.");

            var item = new Market.Domain.Entities.InventoryItem.InventoryItem
            {
                Name = r.Name.Trim(),
                Sku = r.Sku,
                RestaurantId = restaurantId,
                UnitType = r.UnitType,
                ReorderFrequency = r.ReorderFrequency,
                ReorderQty = r.ReorderQty,
                CurrentQty = r.CurrentQty
            };

            db.InventoryItems.Add(item);
            await db.SaveChangesAsync(ct);

            return item.Id;
        }
    }
}