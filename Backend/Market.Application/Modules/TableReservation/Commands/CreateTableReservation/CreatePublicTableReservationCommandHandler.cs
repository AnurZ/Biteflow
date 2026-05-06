using Market.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.TableReservation.Commands.CreateTableReservation
{
    public sealed class CreatePublicTableReservationCommandHandler(
        IAppDbContext db,
        IPublicTenantResolver publicTenantResolver)
        : IRequestHandler<CreatePublicTableReservationCommandDto, int>
    {
        public async Task<int> Handle(CreatePublicTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var publicTenant = await publicTenantResolver.ResolveRequiredAsync(cancellationToken);

            if (request.NumberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than zero.");

            if (request.ReservationEnd.HasValue && request.ReservationStart >= request.ReservationEnd)
                throw new ArgumentException("Reservation start must be before reservation end.");

            var table = await db.DiningTables
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    t => t.Id == request.DiningTableId && t.TenantId == publicTenant.TenantId,
                    cancellationToken);
            if (table == null)
                throw new InvalidOperationException("Dining table not found.");

            if (request.NumberOfGuests > table.NumberOfSeats)
                throw new ArgumentException("Too many guests for this table.");

            var overlappingReservation = await db.TableReservations
                .IgnoreQueryFilters()
                .AnyAsync(r =>
                    r.TenantId == publicTenant.TenantId &&
                    r.DiningTableId == request.DiningTableId &&
                    (
                        (r.ReservationEnd.HasValue && request.ReservationStart < r.ReservationEnd && request.ReservationEnd > r.ReservationStart) ||
                        (!r.ReservationEnd.HasValue && request.ReservationStart < r.ReservationStart)
                    ),
                    cancellationToken);

            if (overlappingReservation)
                throw new InvalidOperationException("The table is already reserved during the requested time.");

            var reservation = new Domain.Entities.TableReservations.TableReservation
            {
                TenantId = publicTenant.TenantId,
                DiningTableId = request.DiningTableId,
                NumberOfGuests = request.NumberOfGuests,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Notes = request.Notes,
                ReservationStart = request.ReservationStart,
                ReservationEnd = request.ReservationEnd,
                Status = ReservationStatus.Pending
            };

            db.TableReservations.Add(reservation);
            await db.SaveChangesAsync(cancellationToken);

            return reservation.Id;
        }
    }
}
