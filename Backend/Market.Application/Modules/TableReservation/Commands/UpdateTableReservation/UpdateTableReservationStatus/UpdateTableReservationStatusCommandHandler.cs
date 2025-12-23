using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation.UpdateTableReservationStatus
{
    public sealed class UpdateTableReservationStatusCommandHandler
    : IRequestHandler<UpdateTableReservationStatusDto>
    {
        private readonly IAppDbContext _db;

        public UpdateTableReservationStatusCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(UpdateTableReservationStatusDto request, CancellationToken cancellationToken)
        {
            var reservation = await _db.TableReservations
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {request.Id} not found.");

            reservation.Status = (Domain.Common.Enums.ReservationStatus)request.Status;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

}
