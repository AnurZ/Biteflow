using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.DeleteTableReservation
{
    public sealed class DeleteTableReservationCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        : IRequestHandler<DeleteTableReservationCommandDto>
    {
        public async Task Handle(DeleteTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.IsSuperAdmin
                ? (Guid?)null
                : tenantContext.RequireRestaurantId();

            var query = db.TableReservations
                .Include(x => x.DiningTable)
                .ThenInclude(x => x!.TableLayout)
                .WhereTenantOwned(tenantContext);

            if (restaurantId.HasValue)
            {
                query = query.Where(x => x.DiningTable != null &&
                                         x.DiningTable.TableLayout.RestaurantId == restaurantId.Value);
            }

            var tr = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (tr is null) 
                throw new KeyNotFoundException($"Table reservation with ID {request.Id} not found.");

            db.TableReservations.Remove(tr);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
