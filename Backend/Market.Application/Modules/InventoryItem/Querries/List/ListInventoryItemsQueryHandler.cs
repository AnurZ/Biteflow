using Market.Application.Abstractions;
using Market.Application.Modules.InventoryItem.Queries.List;
using Market.Application.Modules.InventoryItem.Querries.List;
using Market.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class ListInventoryItemsQueryHandler : IRequestHandler<ListInventoryItemsQuery, PageResult<ListInventoryItemsDto>>
{
    private readonly IAppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ListInventoryItemsQueryHandler(IAppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<PageResult<ListInventoryItemsDto>> Handle(ListInventoryItemsQuery req, CancellationToken ct)
    {
        var restaurantId = _tenantContext.RestaurantId;

        if (restaurantId == null || restaurantId == Guid.Empty)
            throw new ValidationException("Restaurant context is missing.");

        var q = _db.InventoryItems
            .AsNoTracking()
            .Where(x => x.RestaurantId == restaurantId)
            .Select(x => new ListInventoryItemsDto
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

        // Search
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var s = req.Search.Trim().ToLower();
            q = q.Where(x =>
                x.Name.ToLower().Contains(s) ||
                x.Sku.ToLower().Contains(s));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(req.Sort))
        {
            bool desc = req.Sort.StartsWith("-");
            string key = (desc ? req.Sort[1..] : req.Sort).ToLower();

            q = key switch
            {
                "name" => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
                "currentqty" => desc ? q.OrderByDescending(x => x.CurrentQty) : q.OrderBy(x => x.CurrentQty),
                _ => q.OrderBy(x => x.Id)
            };
        }
        else
        {
            q = q.OrderBy(x => x.Id);
        }

        // Pagination
        return await PageResult<ListInventoryItemsDto>.FromQueryableAsync(q, req.Paging, ct);
    }
}
