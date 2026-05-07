using Market.Application.Abstractions;
using Market.Domain.Common.Enums;
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
              .Include(x => x.Meal)
              .Where(oi => !oi.Order.IsDeleted
              && oi.Order.Status != OrderStatus.Cancelled
              && oi.Order.TenantId == _tenantContext.TenantId);

            if (request.From.HasValue)
                query = query.Where(x => x.Order.CreatedAtUtc >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(x => x.Order.CreatedAtUtc <= request.To.Value);

            var data = await query
                .GroupBy(x => x.MealId)
                .Select(g => new GetTopSellingItemsDto
                {
                    ItemName = g.First().Meal != null ? g.First().Meal.Name : "Custom Item",
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(request.TopN)
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}