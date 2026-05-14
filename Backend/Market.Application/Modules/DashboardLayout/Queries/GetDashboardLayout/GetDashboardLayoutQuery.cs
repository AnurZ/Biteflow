using Market.Application.Features.DashboardLayouts.DTOs;
using MediatR;

namespace Market.Application.Features.DashboardLayouts.Queries.GetDashboardLayout
{
    public class GetDashboardLayoutQuery
        : IRequest<DashboardLayoutDto>
    {
        public Guid ApplicationUserId { get; set; }
    }
}