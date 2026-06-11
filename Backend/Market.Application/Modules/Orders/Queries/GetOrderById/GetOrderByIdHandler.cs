using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdHandler
        : IRequestHandler<GetOrderByIdQuery, OrderByIdDto>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenant;

        public GetOrderByIdHandler(IAppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<OrderByIdDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
        {
            var tenantId = _tenant.RequireTenantId();

            var order = await _db.Orders
                .AsNoTracking()
                .Where(o => o.Id == request.Id && o.TenantId == tenantId)
                .Include(o => o.Items)
                .Select(o => new OrderByIdDto
                {
                    Id = o.Id,
                    TableLayoutId = o.DiningTable.TableLayoutId,
                    TableLayoutName = o.DiningTable.TableLayout.Name,
                    DiningTableId = o.DiningTableId,
                    TableNumber = o.TableNumber,
                    Status = o.Status,
                    CreatedAtUtc = o.CreatedAtUtc,
                    Notes = o.Notes,

                    ItemsCount = o.Items.Sum(i => i.Quantity),
                    TotalPrice = o.Items.Sum(i => i.Quantity * i.UnitPrice),

                    Items = o.Items.Select(i => new OrderByIdItemDto
                    {
                        Id = i.Id,
                        MealId = i.MealId,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (order == null)
                throw new Exception($"Order {request.Id} not found");

            return order;
        }
    }
}