using Market.Application.Abstractions;
using Market.Application.Modules.InventoryItem.Querries.GetByName;
using Market.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Queries.GetByName
{
    public sealed class GetInventoryItemByNameQueryHandler
        : IRequestHandler<GetInventoryItemByNameQuery, PageResult<GetInventoryItemByNameDto>>
    {
        private readonly IAppDbContext _db;

        public GetInventoryItemByNameQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<PageResult<GetInventoryItemByNameDto>> Handle(GetInventoryItemByNameQuery req, CancellationToken ct)
        {
            var q = _db.InventoryItems
                .AsNoTracking()
                .Select(x => new GetInventoryItemByNameDto
                {
                    Id = x.Id,
                    RestaurantId = x.RestaurantId,
                    Name = x.Name,
                    Sku = x.Sku,
                    UnitType = x.UnitType,
                    ReorderQty = x.ReorderQty,
                    ReorderFrequency = x.ReorderFrequency,
                    CurrentQty = x.CurrentQty
                });

            // Filter: items whose names start with input
            if (!string.IsNullOrWhiteSpace(req.Name))
            {
                var s = req.Name.Trim().ToLower();
                q = q.Where(x => x.Name.ToLower().StartsWith(s));
            }

            // Pagination (same helper)
            return await PageResult<GetInventoryItemByNameDto>.FromQueryableAsync(q, req.Paging, ct);
        }
    }
}
