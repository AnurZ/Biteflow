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

    public ListInventoryItemsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<PageResult<ListInventoryItemsDto>> Handle(ListInventoryItemsQuery req, CancellationToken ct)
    {
        var q = _db.InventoryItems
            .AsNoTracking()
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
            string key = desc ? req.Sort[1..] : req.Sort;

            q = key switch
            {
                "name" => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
                "currentQty" => desc ? q.OrderByDescending(x => x.CurrentQty) : q.OrderBy(x => x.CurrentQty),
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
