using Market.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.InventoryItem.Queries.GetByName
{
    public sealed class GetInventoryItemByNameQueryHandler
        : IRequestHandler<GetInventoryItemByNameQuery, PageResult<GetInventoryItemByNameDto>>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public GetInventoryItemByNameQueryHandler(
            IAppDbContext db,
            ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<PageResult<GetInventoryItemByNameDto>> Handle(
            GetInventoryItemByNameQuery req,
            CancellationToken ct)
        {
            var restaurantId = _tenantContext.RequireRestaurantId();

            var q = _db.InventoryItems
                .AsNoTracking()
                .Where(x => x.RestaurantId == restaurantId)
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

            q = q.OrderBy(x => x.Name);

            // Pagination
            return await PageResult<GetInventoryItemByNameDto>
                .FromQueryableAsync(q, req.Paging, ct);
        }
    }
}