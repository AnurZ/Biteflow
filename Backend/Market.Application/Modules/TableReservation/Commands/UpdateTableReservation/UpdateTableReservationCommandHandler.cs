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

        public UpdateTableReservationCommandHandler(
            IAppDbContext db,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
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
            }

            var reservation = await _db.TableReservations
                .Include(r => r.DiningTable)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            // Validations
            if (request.NumberOfGuests <= 0)
                throw new ValidationException("Number of guests must be greater than zero.");

            if (request.ReservationEnd.HasValue && request.ReservationStart >= request.ReservationEnd)
                throw new ValidationException("Reservation start must be before reservation end.");

            var newTable = await _db.DiningTables
                .FirstOrDefaultAsync(t => t.Id == request.DiningTableId, cancellationToken);

            if (newTable == null)
                throw new KeyNotFoundException($"Dining table with ID {request.DiningTableId} not found.");

            if (request.NumberOfGuests > newTable.NumberOfSeats)
                throw new ValidationException($"Too many guests ({request.NumberOfGuests}) for this table (Id: {newTable.Id} ({newTable.NumberOfSeats})).");

            // Overlapping reservations (consider nullable ReservationEnd)
            bool overlapping = await _db.TableReservations
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
