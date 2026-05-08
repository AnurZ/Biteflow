using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.TableReservations;
using Market.Application.Modules.TableReservation.Commands.CreateTableReservation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Market.Domain.Common.Enums;

namespace Market.Application.Modules.TableReservation.Commands.CreateTableReservation
{
    public sealed class CreateTableReservationCommandHandler : IRequestHandler<CreateTableReservationCommandDto, int>
    {
        private readonly IAppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantContext _tenantContext;

        public CreateTableReservationCommandHandler(
            IAppDbContext db,
            UserManager<ApplicationUser> userManager,
            ITenantContext tenantContext)
        {
            _db = db;
            _userManager = userManager;
            _tenantContext = tenantContext;
        }

        public async Task<int> Handle(CreateTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantContext.RequireTenantId();
            var restaurantId = _tenantContext.RequireRestaurantId();

            // Optional user lookup
            ApplicationUser? user = null;
            if (request.ApplicationUserId.HasValue)
            {
                user = await _userManager.FindByIdAsync(request.ApplicationUserId.Value.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {request.ApplicationUserId} not found.");
                if (user.TenantId != tenantId)
                    throw new InvalidOperationException("User does not belong to the current tenant.");
            }

            // Basic validation
            if (request.NumberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than zero.");

            if (request.ReservationEnd.HasValue && request.ReservationStart >= request.ReservationEnd)
                throw new ArgumentException("Reservation start must be before reservation end.");

            // Find table
            var table = await _db.DiningTables
                .Include(t => t.TableLayout)
                .FirstOrDefaultAsync(t =>
                    t.Id == request.DiningTableId &&
                    t.TableLayout.RestaurantId == restaurantId,
                    cancellationToken);
            if (table == null)
                throw new InvalidOperationException("Dining table not found.");

            if (request.NumberOfGuests > table.NumberOfSeats)
                throw new ArgumentException("Too many guests for this table.");

            // Check overlapping reservations
            var overlappingReservation = await _db.TableReservations
                .AnyAsync(r =>
                     r.DiningTableId == request.DiningTableId &&
                     r.Status != ReservationStatus.Cancelled &&
                     request.ReservationStart < (r.ReservationEnd ?? DateTime.MaxValue) &&
                     r.ReservationStart < (request.ReservationEnd ?? DateTime.MaxValue),
                     cancellationToken);

            if (overlappingReservation)
                throw new InvalidOperationException("The table is already reserved during the requested time.");

            // Create reservation
            var reservation = new Domain.Entities.TableReservations.TableReservation
            {
                DiningTableId = request.DiningTableId,
                TenantId = tenantId,
                NumberOfGuests = request.NumberOfGuests,
                ApplicationUserId = request.ApplicationUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Notes = request.Notes,
                ReservationStart = request.ReservationStart,
                ReservationEnd = request.ReservationEnd,
                Status = Domain.Common.Enums.ReservationStatus.Pending
            };

            _db.TableReservations.Add(reservation);
            await _db.SaveChangesAsync(cancellationToken);

            return reservation.Id;
        }
    }
}
