using Market.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Analytics.Queries.GetTopSellingItems
{
    public class GetTopSellingItemsHandler
        : IRequestHandler<GetTopSellingItemsQuery, List<GetTopSellingItemsDto>>
    {
        private readonly IAppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public GetTopSellingItemsHandler(
            IAppDbContext context,
            ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<List<GetTopSellingItemsDto>> Handle(
            GetTopSellingItemsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.OrderItems
                .AsNoTracking()
                .Where(oi => !oi.Order.IsDeleted
                             && oi.Order.TenantId == _tenantContext.TenantId);

            if (request.From.HasValue)
                query = query.Where(x => x.Order.CreatedAtUtc >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(x => x.Order.CreatedAtUtc <= request.To.Value);

            var data = await query
                .GroupBy(x => x.Name)
                .Select(g => new GetTopSellingItemsDto
                {
                    ItemName = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}