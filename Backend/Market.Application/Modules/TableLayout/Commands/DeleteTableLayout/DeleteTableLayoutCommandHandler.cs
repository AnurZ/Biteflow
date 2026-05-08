using Market.Domain.Entities.TableLayout;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.TableLayout.Commands.DeleteTableLayout
{
    public sealed class DeleteTableLayoutCommandHandler : IRequestHandler<DeleteTableLayoutCommandDto>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public DeleteTableLayoutCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task Handle(DeleteTableLayoutCommandDto request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantContext.RequireTenantId();
            var restaurantId = _tenantContext.RequireRestaurantId();

            var layout = await _db.TableLayouts
                .Include(t => t.Tables)
                .Where(t => t.TenantId == tenantId && t.RestaurantId == restaurantId)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (layout == null)
                throw new KeyNotFoundException($"TableLayout with ID {request.Id} not found.");

            if (layout.Tables.Any())
                throw new InvalidOperationException("Cannot delete a layout that has tables assigned.");

            _db.TableLayouts.Remove(layout);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
