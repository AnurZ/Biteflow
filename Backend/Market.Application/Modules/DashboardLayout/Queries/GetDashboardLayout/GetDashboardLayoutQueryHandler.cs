using Market.Application.Features.DashboardLayouts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Features.DashboardLayouts.Queries.GetDashboardLayout
{
    public class GetDashboardLayoutQueryHandler
        : IRequestHandler<
            GetDashboardLayoutQuery,
            DashboardLayoutDto>
    {
        private readonly IAppDbContext _context;

        public GetDashboardLayoutQueryHandler(
            IAppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardLayoutDto> Handle(
            GetDashboardLayoutQuery request,
            CancellationToken cancellationToken)
        {
            var layout = await _context.DashboardLayouts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ApplicationUserId == request.ApplicationUserId,
                    cancellationToken);

            if (layout == null)
            {
                return new DashboardLayoutDto
                {
                    LayoutJson = """
                        {
                          "kpis": ["orders", "revenue", "avgOrder", "topItem"],
                          "widgets": ["liveOrders", "charts"],
                          "charts": ["revenue", "orders", "topSelling"]
                        }
                        """
                };
            }

            return new DashboardLayoutDto
            {
                LayoutJson = layout.LayoutJson
            };
        }
    }
}