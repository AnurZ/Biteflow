using Market.Domain.Entities.TableReservations;
using Market.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Market.Domain.Entities.IdentityV2;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation
{
    public sealed class UpdateTableReservationCommandHandler : IRequestHandler<UpdateTableReservationCommandDto>
    {
        private readonly IAppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantContext _tenantContext;

        public UpdateTableReservationCommandHandler(
            IAppDbContext db,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            ITenantContext tenantContext)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _tenantContext = tenantContext;
        }

        public async Task Handle(UpdateTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            // Optional user lookup
            ApplicationUser? user = null;
            if (request.ApplicationUserId.HasValue)
            {
                user = await _userManager.FindByIdAsync(request.ApplicationUserId.Value.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {request.ApplicationUserId} not found.");

                if (!_tenantContext.IsSuperAdmin &&
                    (user.TenantId != _tenantContext.RequireTenantId() ||
                     user.RestaurantId != _tenantContext.RequireRestaurantId()))
                {
                    throw new KeyNotFoundException($"User with ID {request.ApplicationUserId} not found.");
                }
            }

            var restaurantId = _tenantContext.IsSuperAdmin
                ? (Guid?)null
                : _tenantContext.RequireRestaurantId();

            var reservationQuery = _db.TableReservations
                .Include(r => r.DiningTable)
                .ThenInclude(t => t!.TableLayout)
                .WhereTenantOwned(_tenantContext);

            if (restaurantId.HasValue)
            {
                reservationQuery = reservationQuery
                    .Where(r => r.DiningTable != null &&
                                r.DiningTable.TableLayout.RestaurantId == restaurantId.Value);
            }

            var reservation = await reservationQuery
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            // Validations
            if (request.NumberOfGuests <= 0)
                throw new ValidationException("Number of guests must be greater than zero.");

            if (request.ReservationEnd.HasValue && request.ReservationStart >= request.ReservationEnd)
                throw new ValidationException("Reservation start must be before reservation end.");

            var newTableQuery = _db.DiningTables
                .Include(t => t.TableLayout)
                .WhereTenantOwned(_tenantContext);

            if (restaurantId.HasValue)
            {
                newTableQuery = newTableQuery.Where(t => t.TableLayout.RestaurantId == restaurantId.Value);
            }

            var newTable = await newTableQuery
                .FirstOrDefaultAsync(t => t.Id == request.DiningTableId, cancellationToken);

            if (newTable == null)
                throw new KeyNotFoundException($"Dining table with ID {request.DiningTableId} not found.");

            if (request.NumberOfGuests > newTable.NumberOfSeats)
                throw new ValidationException($"Too many guests ({request.NumberOfGuests}) for this table (Id: {newTable.Id} ({newTable.NumberOfSeats})).");

            // Overlapping reservations (consider nullable ReservationEnd)
            bool overlapping = await _db.TableReservations
                .WhereTenantOwned(_tenantContext)
                .Where(r => r.DiningTableId == request.DiningTableId && r.Id != request.Id)
                .AnyAsync(r =>
                    (r.ReservationEnd.HasValue
                        ? request.ReservationStart < r.ReservationEnd && request.ReservationEnd > r.ReservationStart
                        : request.ReservationStart < r.ReservationStart),
                    cancellationToken);

            if (overlapping)
                throw new ValidationException("The table is already reserved during the requested time.");

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
