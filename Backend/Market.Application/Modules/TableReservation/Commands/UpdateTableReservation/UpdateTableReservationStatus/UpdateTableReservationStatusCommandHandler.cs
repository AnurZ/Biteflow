using Market.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation.UpdateTableReservationStatus
{
    public sealed class UpdateTableReservationStatusCommandHandler
        : IRequestHandler<UpdateTableReservationStatusDto>
    {
        private static readonly Dictionary<ReservationStatus, ReservationStatus[]> AllowedTransitions = new()
        {
            [ReservationStatus.Pending] = new[] { ReservationStatus.Confirmed, ReservationStatus.Cancelled },
            [ReservationStatus.Confirmed] = new[] { ReservationStatus.Cancelled },
            [ReservationStatus.Cancelled] = Array.Empty<ReservationStatus>()
        };

        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public UpdateTableReservationStatusCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task Handle(UpdateTableReservationStatusDto request, CancellationToken cancellationToken)
        {
            var reservation = await _db.TableReservations
                .WhereTenantOwned(_tenantContext)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            if (reservation.Status == ReservationStatus.Cancelled)
                throw new ValidationException("Cancelled reservation cannot be modified.");

            if (!AllowedTransitions[reservation.Status].Contains(request.Status))
                throw new ValidationException(
                    $"Cannot change status from {reservation.Status} to {request.Status}");

            reservation.Status = request.Status;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}