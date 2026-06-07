using Market.Application.Common;
using Market.Application.Modules.Meal.Queries.GetList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Orders.Queries.AdminGetOrders
{
    public class AdminGetOrdersHandler(IAppDbContext db, ITenantContext tenantContext)
        : IRequestHandler<AdminGetOrdersQuery, PageResult<AdminOrderDto>>
    {
        public async Task<PageResult<AdminOrderDto>> Handle(AdminGetOrdersQuery request, CancellationToken cancellationToken)
        {
            var q = db.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .AsQueryable();

            // -------------------
            // STATUS FILTER
            // -------------------
            if (request.Statuses?.Any() == true)
            {
                q = q.Where(x => request.Statuses.Contains(x.Status));
            }

            // -------------------
            // DATE FILTER
            // -------------------
            if (request.FromUtc.HasValue)
            {
                q = q.Where(x => x.CreatedAtUtc >= request.FromUtc.Value);
            }

            if (request.ToUtc.HasValue)
            {
                q = q.Where(x => x.CreatedAtUtc <= request.ToUtc.Value);
            }

            // -------------------
            // PROJECTION 
            // -------------------
            var projected = q.Select(o => new AdminOrderDto
            {
                Id = o.Id,
                DiningTableId = o.DiningTableId,
                TableNumber = o.TableNumber,
                Status = o.Status,
                CreatedAtUtc = o.CreatedAtUtc,
                Notes = o.Notes,

                ItemsCount = o.Items.Sum(i => i.Quantity),

                TotalPrice = o.Items.Sum(i => i.Quantity * i.UnitPrice),

                Items = o.Items.Select(i => new AdminOrderItemDto
                {
                    Id = i.Id,
                    MealId = i.MealId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });

            // -------------------
            // SORT
            // -------------------
            if (!string.IsNullOrWhiteSpace(request.Sort))
            {
                bool desc = request.Sort.StartsWith("-");
                string key = (desc ? request.Sort[1..] : request.Sort).ToLower();

                projected = key switch
                {
                    // TIME
                    "createdat" => desc
                        ? projected.OrderByDescending(x => x.CreatedAtUtc)
                        : projected.OrderBy(x => x.CreatedAtUtc),

                    // TOTAL PRICE
                    "totalprice" => desc
                        ? projected.OrderByDescending(x => x.TotalPrice)
                        : projected.OrderBy(x => x.TotalPrice),

                    // ITEMS COUNT
                    "itemscount" => desc
                        ? projected.OrderByDescending(x => x.ItemsCount)
                        : projected.OrderBy(x => x.ItemsCount),

                    // STATUS
                    "status" => desc
                        ? projected.OrderByDescending(x => x.Status)
                        : projected.OrderBy(x => x.Status),

                    // ID
                    "id" => desc
                        ? projected.OrderByDescending(x => x.Id)
                        : projected.OrderBy(x => x.Id),

                    _ => projected.OrderByDescending(x => x.CreatedAtUtc)
                };
            }
            else
            {
                projected = projected.OrderByDescending(x => x.CreatedAtUtc);
            }

            // -------------------
            // PAGINATION
            // -------------------
            return await PageResult<AdminOrderDto>.FromQueryableAsync(projected, request.Paging, cancellationToken);
        }
    }
}
