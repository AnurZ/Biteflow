using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Features.DashboardLayouts.Commands.SaveDashboardLayout
{
    public class SaveDashboardLayoutCommandHandler
        : IRequestHandler<SaveDashboardLayoutCommand, Unit>
    {
        private readonly IAppDbContext _context;

        public SaveDashboardLayoutCommandHandler(
            IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(
            SaveDashboardLayoutCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _context.DashboardLayouts
                .FirstOrDefaultAsync(
                    x => x.ApplicationUserId == request.ApplicationUserId,
                    cancellationToken);

            // CREATE
            if (existing == null)
            {
                existing = new Domain.Entities.DashboardLayout.DashboardLayout
                {
                    ApplicationUserId = request.ApplicationUserId,
                    LayoutJson = request.LayoutJson
                };

                _context.DashboardLayouts.Add(existing);
            }

            // UPDATE
            else
            {
                existing.LayoutJson = request.LayoutJson;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}