using Market.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderDto>>
    {
        private readonly IAppDbContext _db;

        public GetOrdersQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAtUtc)
                .AsQueryable();

            if (request.Statuses is { Count: > 0 })
            {
                query = query.Where(o => request.Statuses!.Contains(o.Status));
            }

            return await query
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    DiningTableId = o.DiningTableId,
                    TableNumber = o.TableNumber,
                    Status = o.Status,
                    CreatedAtUtc = o.CreatedAtUtc,
                    Notes = o.Notes,
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        MealId = i.MealId,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}
