using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation.UpdateTableReservationStatus
{
    public sealed class UpdateTableReservationStatusCommandHandler
    : IRequestHandler<UpdateTableReservationStatusDto>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public UpdateTableReservationStatusCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task Handle(UpdateTableReservationStatusDto request, CancellationToken cancellationToken)
        {
            var restaurantId = _tenantContext.IsSuperAdmin
                ? (Guid?)null
                : _tenantContext.RequireRestaurantId();

            var query = _db.TableReservations
                .Include(r => r.DiningTable)
                .ThenInclude(t => t!.TableLayout)
                .WhereTenantOwned(_tenantContext);

            if (restaurantId.HasValue)
            {
                query = query.Where(r => r.DiningTable != null &&
                                         r.DiningTable.TableLayout.RestaurantId == restaurantId.Value);
            }

            var reservation = await query.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            reservation.Status = (Domain.Common.Enums.ReservationStatus)request.Status;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

}
