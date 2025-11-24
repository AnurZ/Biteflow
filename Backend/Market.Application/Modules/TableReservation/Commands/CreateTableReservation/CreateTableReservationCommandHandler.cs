using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.CreateTableReservation
{
    public sealed class CreateTableReservationCommandHandler : IRequestHandler<CreateTableReservationCommandDto, int>
    {
        private readonly IAppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateTableReservationCommandHandler(IAppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<int> Handle(CreateTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.ApplicationUserId.ToString());

            if (user == null)
                throw new KeyNotFoundException($"User with ID {request.ApplicationUserId} not found.");

            // Basic validation (move to FluentValidation if possible)
            if (request.NumberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than zero.");

            if (request.ReservationStart >= request.ReservationEnd)
                throw new ArgumentException("Reservation start must be before reservation end.");

            var table = await _db.DiningTables.FindAsync(new object[] { request.DiningTableId }, cancellationToken);
            if (table == null)
                throw new InvalidOperationException("Dining table not found.");

            var overlappingReservation = await _db.TableReservations
                .AnyAsync(r =>
                    r.DiningTableId == request.DiningTableId &&
                    request.ReservationStart < r.ReservationEnd &&
                    request.ReservationEnd > r.ReservationStart,
                    cancellationToken);

            if (overlappingReservation)
                throw new InvalidOperationException("The table is already reserved during the requested time.");

            if (request.NumberOfGuests > table.NumberOfSeats)
                throw new ValidationException("Too many guests for this table.");

            var reservation = new Domain.Entities.TableReservations.TableReservation
            {
                DiningTableId = request.DiningTableId,
                NumberOfGuests = request.NumberOfGuests,
                ApplicationUserId = request.ApplicationUserId,
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
