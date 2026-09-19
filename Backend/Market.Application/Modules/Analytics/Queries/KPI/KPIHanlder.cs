using Market.Application.Abstractions;
using Market.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Analytics.Queries.KPI
{
    public class GetKpisHandler : IRequestHandler<GetKpisQuery, KpiDto>
    {
        private readonly IAppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public GetKpisHandler(
            IAppDbContext context,
            ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<KpiDto> Handle(
            GetKpisQuery request,
            CancellationToken cancellationToken)
        {
            var restaurantId = _tenantContext.RequireRestaurantId();

            var from = request.From;
            var to = request.To;

            var ordersQuery = _context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.RestaurantId == restaurantId &&
                    o.Status != OrderStatus.Cancelled &&
                    o.CreatedAtUtc >= from &&
                    o.CreatedAtUtc < to);

            var totalOrders = await ordersQuery
                .CountAsync(cancellationToken);

            var revenue = await _context.OrderItems
                .AsNoTracking()
                .Where(i =>
                    i.Order.RestaurantId == restaurantId &&
                    i.Order.Status != OrderStatus.Cancelled &&
                    i.Order.CreatedAtUtc >= from &&
                    i.Order.CreatedAtUtc < to)
                .SumAsync(
                    i => (decimal?)i.UnitPrice * i.Quantity,
                    cancellationToken
                ) ?? 0;

            var avgOrder = totalOrders > 0
                ? Math.Round(
                    revenue / totalOrders,
                    2,
                    MidpointRounding.AwayFromZero)
                : 0;

            var topItem = await _context.OrderItems
                .AsNoTracking()
                .Where(i =>
                    i.Order.RestaurantId == restaurantId &&
                    i.Order.Status != OrderStatus.Cancelled &&
                    i.Order.CreatedAtUtc >= from &&
                    i.Order.CreatedAtUtc < to)
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
