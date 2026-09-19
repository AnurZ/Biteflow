using Market.Application.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Queries.GetById
{
    public class GetInventoryItemByIdQueryHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
        : IRequestHandler<GetInventoryItemByIdQuery, GetInventoryItemByIdDto>
    {
        public async Task<GetInventoryItemByIdDto> Handle(
            GetInventoryItemByIdQuery req,
            CancellationToken ct)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var ie = await db.InventoryItems
                .FirstOrDefaultAsync(
                    x => x.Id == req.Id &&
                         x.RestaurantId == restaurantId,
                    ct);

            if (ie is null)
                throw new KeyNotFoundException("InventoryItem");

            return new GetInventoryItemByIdDto
            {
                RestaurantId = ie.RestaurantId,
                ReorderFrequency = ie.ReorderFrequency,
                ReorderQty = ie.ReorderQty,
                CurrentQty = ie.CurrentQty,
                Name = ie.Name,
                Sku = ie.Sku,
                UnitType = ie.UnitType
            };
        }
    }
}