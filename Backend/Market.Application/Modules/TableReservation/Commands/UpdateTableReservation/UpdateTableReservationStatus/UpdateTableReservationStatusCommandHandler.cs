using Market.Domain.Common.Enums;
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

            var reservation = await _db.TableReservations
                .Include(r => r.DiningTable)
                .ThenInclude(t => t!.TableLayout)
                .WhereTenantOwned(_tenantContext)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            if (reservation.Status == ReservationStatus.Cancelled)
                throw new ValidationException("Cancelled reservation cannot be modified.");

            if (reservation.DiningTable?.TableLayout?.RestaurantId != restaurantId)
                throw new UnauthorizedAccessException();

            var allowedTransitions = new Dictionary<ReservationStatus, ReservationStatus[]>
            {
                [ReservationStatus.Pending] = new[] { ReservationStatus.Confirmed, ReservationStatus.Cancelled },
                [ReservationStatus.Confirmed] = new[] { ReservationStatus.Cancelled },
                [ReservationStatus.Cancelled] = Array.Empty<ReservationStatus>()
            };

            if (!allowedTransitions[reservation.Status].Contains(request.Status))
                throw new ValidationException($"Cannot change status from {reservation.Status} to {request.Status}");

            reservation.Status = request.Status;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

}
