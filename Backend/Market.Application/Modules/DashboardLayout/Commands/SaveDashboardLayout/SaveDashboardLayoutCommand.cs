using MediatR;

namespace Market.Application.Features.DashboardLayouts.Commands.SaveDashboardLayout
{
    public class SaveDashboardLayoutCommand : IRequest<Unit>
    {
        public Guid ApplicationUserId { get; set; }

        public string LayoutJson { get; set; } = string.Empty;
    }
}