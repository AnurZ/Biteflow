using Microsoft.AspNetCore.Http;
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
        private readonly IAppCurrentUser _currentUser;

        public CreateTableReservationCommandHandler(IAppDbContext db, IAppCurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(CreateTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            if (request.NumberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than zero.");

            if (request.ReservationStart >= request.ReservationEnd)
                throw new ArgumentException("Reservation start must be before reservation end.");

            var table = await _db.DiningTables.FindAsync(new object[] { request.DiningTableId }, cancellationToken);
            if (table == null)
                throw new InvalidOperationException("Dining table not found.");

            var overlappingReservation = _db.TableReservations
                .Where(r => r.DiningTableId == request.DiningTableId)
                .Any(r =>
                    (request.ReservationStart < r.ReservationEnd) &&
                    (request.ReservationEnd > r.ReservationStart)
                ); 
            Console.WriteLine(".--------------------------------------------------------------------------");

            Console.WriteLine(_currentUser.UserId);

            if (overlappingReservation)
                throw new InvalidOperationException("The table is already reserved during the requested time.");


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
