using Market.Application.Abstractions;
using Market.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Analytics.Queries.GetRevenuePerDay
{
    public class GetRevenuePerDayHandler
        : IRequestHandler<GetRevenuePerDayQuery, List<GetRevenuePerDayDto>>
    {
        private readonly IAppDbContext _context;

        public GetRevenuePerDayHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<GetRevenuePerDayDto>> Handle(
            GetRevenuePerDayQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled);

            if (request.From.HasValue)
                query = query.Where(x => x.Order.CreatedAtUtc >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(x => x.Order.CreatedAtUtc <= request.To.Value);

            var data = await query
                .GroupBy(x => x.Order.CreatedAtUtc.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .ToListAsync(cancellationToken);

            return data
                .Select(x => new GetRevenuePerDayDto
                {
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    Revenue = x.Revenue
                })
                .OrderBy(x => x.Date)
                .ToList();
        }
    }
}
