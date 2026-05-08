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

            // Public reservations are created before an authenticated tenant context exists.
            var table = await db.DiningTables
                // Public reservation lookup runs before an authenticated tenant context exists.
                .IgnoreQueryFilters()
                .Include(t => t.TableLayout)
                .FirstOrDefaultAsync(
                    t => t.Id == request.DiningTableId &&
                         t.TenantId == publicTenant.TenantId &&
                         t.TableLayout.RestaurantId == publicTenant.RestaurantId,
                    cancellationToken);
            if (table == null)
                throw new InvalidOperationException("Dining table not found.");

            if (request.NumberOfGuests > table.NumberOfSeats)
                throw new ArgumentException("Too many guests for this table.");

            var overlappingReservation = await db.TableReservations
                // Public reservations must check tenant rows without an authenticated global filter.
                .IgnoreQueryFilters()
                .AnyAsync(r =>
                    r.TenantId == publicTenant.TenantId &&
                    r.DiningTableId == request.DiningTableId &&
                    r.Status != ReservationStatus.Cancelled &&
                    request.ReservationStart < (r.ReservationEnd ?? DateTime.MaxValue) &&
                    r.ReservationStart < (request.ReservationEnd ?? DateTime.MaxValue),
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
