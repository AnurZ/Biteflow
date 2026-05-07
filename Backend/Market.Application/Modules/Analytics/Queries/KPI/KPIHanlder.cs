using Market.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Analytics.Queries.KPI
{
    public class GetKpisHandler : IRequestHandler<GetKpisQuery, KpiDto>
    {
        private readonly IAppDbContext _context;

        public GetKpisHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<KpiDto> Handle(GetKpisQuery request, CancellationToken cancellationToken)
        {
            var fromDate = request.From;
            var toDate = request.To;

            var ordersQuery = _context.Orders
                .Where(o => o.CreatedAtUtc >= fromDate && o.CreatedAtUtc <= toDate);

            var totalOrders = await ordersQuery.CountAsync(cancellationToken);

            var revenue = await _context.OrderItems
                .Where(i => i.Order.CreatedAtUtc >= fromDate &&
                            i.Order.CreatedAtUtc <= toDate)
                .SumAsync(i => (decimal?)i.UnitPrice * i.Quantity, cancellationToken) ?? 0;

            var avgOrder = totalOrders > 0 ? revenue / totalOrders : 0;

            var topItem = await _context.OrderItems
                .Where(i => i.Order.CreatedAtUtc >= fromDate &&
                            i.Order.CreatedAtUtc <= toDate)
                .GroupBy(i => i.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);

            return new KpiDto
            {
                TotalOrders = totalOrders,
                Revenue = revenue,
                AvgOrderValue = avgOrder,
                TopItem = topItem ?? "-"
            };
        }
    }
}