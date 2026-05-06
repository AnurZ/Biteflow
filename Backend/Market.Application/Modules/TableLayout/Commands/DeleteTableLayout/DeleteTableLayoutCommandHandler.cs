using Market.Domain.Entities.TableLayout;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

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
            var restaurantId = _tenantContext.IsSuperAdmin
                ? (Guid?)null
                : _tenantContext.RequireRestaurantId();

            var query = _db.TableLayouts
                .Include(t => t.Tables)
                .WhereTenantOwned(_tenantContext);

            if (restaurantId.HasValue)
            {
                query = query.Where(x => x.RestaurantId == restaurantId.Value);
            }

            var layout = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (layout == null)
                throw new KeyNotFoundException($"TableLayout with ID {request.Id} not found.");

            if (layout.Tables != null && layout.Tables.Count > 0)
                throw new InvalidOperationException("Cannot delete a layout that has tables assigned.");

            _db.TableLayouts.Remove(layout);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
