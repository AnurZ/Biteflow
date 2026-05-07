using Market.Domain.Common.Enums;
using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.TableReservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation
{
    public sealed class UpdateTableReservationCommandHandler
        : IRequestHandler<UpdateTableReservationCommandDto>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public UpdateTableReservationCommandHandler(
            IAppDbContext db,
            ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task Handle(UpdateTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var reservation = await _db.TableReservations
                .WhereTenantOwned(_tenantContext)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            if (request.NumberOfGuests <= 0)
                throw new ValidationException("Number of guests must be greater than zero.");

            if (request.ReservationEnd.HasValue &&
                request.ReservationStart >= request.ReservationEnd)
                throw new ValidationException("Reservation start must be before reservation end.");

            var table = await _db.DiningTables
                .WhereTenantOwned(_tenantContext)
                .FirstOrDefaultAsync(t => t.Id == request.DiningTableId, cancellationToken);

            if (table == null)
                throw new KeyNotFoundException($"Dining table with ID {request.DiningTableId} not found.");

            if (request.NumberOfGuests > table.NumberOfSeats)
                throw new ValidationException("Too many guests for this table.");

            // Overlap check (clean version)
            var overlapping = await _db.TableReservations
                .WhereTenantOwned(_tenantContext)
                .Where(r =>
                    r.DiningTableId == request.DiningTableId &&
                    r.Id != request.Id &&
                    r.Status != ReservationStatus.Cancelled)
                .AnyAsync(r =>
                    request.ReservationStart < (r.ReservationEnd ?? DateTime.MaxValue) &&
                    (request.ReservationEnd ?? DateTime.MaxValue) > r.ReservationStart,
                    cancellationToken);

            if (overlapping)
                throw new ValidationException("The table is already reserved in this time slot.");

            // Apply updates
            reservation.DiningTableId = request.DiningTableId;
            reservation.NumberOfGuests = request.NumberOfGuests;
            reservation.ApplicationUserId = request.ApplicationUserId;
            reservation.FirstName = request.FirstName;
            reservation.LastName = request.LastName;
            reservation.Email = request.Email;
            reservation.PhoneNumber = request.PhoneNumber;
            reservation.Notes = request.Notes;
            reservation.ReservationStart = request.ReservationStart;
            reservation.ReservationEnd = request.ReservationEnd;
            reservation.Status = request.Status;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}