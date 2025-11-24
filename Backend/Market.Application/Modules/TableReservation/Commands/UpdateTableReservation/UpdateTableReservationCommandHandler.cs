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

        public UpdateTableReservationCommandHandler(IAppDbContext db, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Handle(UpdateTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.ApplicationUserId.ToString());
            
            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.ApplicationUserId} not found.");

            var reservation = await _db.TableReservations
                .Include(r => r.DiningTable)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            if (request.NumberOfGuests <= 0)
                throw new ValidationException("Number of guests must be greater than zero.");

            if (request.ReservationStart >= request.ReservationEnd)
                throw new ValidationException("Reservation start must be before reservation end.");

            if (request.NumberOfGuests > reservation.DiningTable.NumberOfSeats)
                throw new ValidationException("Too many guests for this table.");

            // Overlapping reservations
            bool overlapping = await _db.TableReservations
                .Where(r => r.DiningTableId == request.DiningTableId && r.Id != request.Id)
                .AnyAsync(r =>
                    request.ReservationStart < r.ReservationEnd &&
                    request.ReservationEnd > r.ReservationStart,
                    cancellationToken);

            if (overlapping)
                throw new ValidationException("The table is already reserved during the requested time.");

            // Apply updates
            reservation.DiningTableId = request.DiningTableId;
            reservation.NumberOfGuests = request.NumberOfGuests;
            reservation.ApplicationUserId = request.ApplicationUserId;
            reservation.Notes = request.Notes;
            reservation.ReservationStart = request.ReservationStart;
            reservation.ReservationEnd = request.ReservationEnd;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

}
