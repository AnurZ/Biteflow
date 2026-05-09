using Market.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Analytics.Queries.GetOrdersPerDay
{
    public class GetOrdersPerDayHandler
        : IRequestHandler<GetOrdersPerDayQuery, List<GetOrdersPerDayDto>>
    {
        private readonly IAppDbContext _context;

        public GetOrdersPerDayHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<GetOrdersPerDayDto>> Handle(
            GetOrdersPerDayQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Orders
                .AsNoTracking();

            if (request.From.HasValue)
                query = query.Where(o => o.CreatedAtUtc >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(o => o.CreatedAtUtc < request.To.Value);

            var data = await query
                .GroupBy(o => new
                {
                    o.CreatedAtUtc.Year,
                    o.CreatedAtUtc.Month,
                    o.CreatedAtUtc.Day
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            return data
                .Select(x => new GetOrdersPerDayDto
                {
                    Date = $"{x.Year}-{x.Month:00}-{x.Day:00}",
                    Count = x.Count
                })
                .OrderBy(x => x.Date)
                .ToList();
        }
    }
}