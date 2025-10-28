using Market.Application.Modules.Staff.Queries.GetById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Querries.GetById
{
    internal class GetInventoryItemByIdQueryHandler(IAppDbContext db)
        : IRequestHandler<GetInventoryItemByIdQuery, GetInventoryItemByIdDto>
    {
        public async Task<GetInventoryItemByIdDto> Handle(GetInventoryItemByIdQuery req, CancellationToken ct)
        {
            var ie = await db.InventoryItems
                .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

            if (ie is null) throw new KeyNotFoundException("InventoryItem");

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
