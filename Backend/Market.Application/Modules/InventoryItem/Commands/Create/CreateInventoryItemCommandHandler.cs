using Market.Application.Modules.Staff.Commands.Create;
using Market.Domain.Entities.Staff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Commands.Create
{
    public sealed class CreateInventoryItemCommandHandler(IAppDbContext db, ITenantContext tenantContext) :
        IRequestHandler<CreateInventoryItemCommand, int>
    {
        public async Task<int> Handle(CreateInventoryItemCommand r, CancellationToken ct)
        {
            var restaurantId = tenantContext.RestaurantId ?? r.RestaurantId;
            if (restaurantId == Guid.Empty)
                throw new ValidationException("Restaurant context is missing.");

            if (string.IsNullOrWhiteSpace(r.Name) || string.IsNullOrWhiteSpace(r.Name))
                throw new ValidationException("Name is required.");
            // Have to add checks for all other attributes

            bool exists = await db.InventoryItems
                .AnyAsync(i => i.Name.ToLower() == r.Name.ToLower()
                            && i.RestaurantId == restaurantId, ct);

            if (exists)
                throw new ValidationException($"An item with the name '{r.Name}' already exists for this restaurant.");


            var ie = new Market.Domain.Entities.InventoryItem.InventoryItem
            {
                Name = r.Name,
                Sku = r.Sku,
                RestaurantId = restaurantId,
                UnitType = r.UnitType,
                ReorderFrequency = r.ReorderFrequency,
                ReorderQty = r.ReorderQty,
                CurrentQty = r.CurrentQty
            };

            db.InventoryItems.Add(ie);
            await db.SaveChangesAsync(ct);
            return ie.Id;
        }
    }
}
