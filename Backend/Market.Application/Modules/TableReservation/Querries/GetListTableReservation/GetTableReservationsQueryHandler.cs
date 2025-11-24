using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Queries.GetTableReservations
{
    public sealed class GetTableReservationsQueryHandler(IAppDbContext db)
        : IRequestHandler<GetTableReservationsQuery, List<GetTableReservationsQueryDto>>
    {
        public async Task<List<GetTableReservationsQueryDto>> Handle(GetTableReservationsQuery request, CancellationToken cancellationToken)
        {
            var query = db.TableReservations.AsQueryable();

            if (request.ReservationId.HasValue)
                query = query.Where(r => r.Id == request.ReservationId.Value);

            if (request.DiningTableId.HasValue)
                query = query.Where(r => r.DiningTableId == request.DiningTableId.Value);

            if (request.RequestedStart.HasValue && request.RequestedEnd.HasValue)
            {
                // Filter for reservations overlapping the requested time
                var start = request.RequestedStart.Value;
                var end = request.RequestedEnd.Value;

                query = query.Where(r => r.ReservationStart < end && r.ReservationEnd > start);
            }

            var reservations = await query
                .Select(r => new GetTableReservationsQueryDto
                {
                    Id = r.Id,
                    DiningTableId = r.DiningTableId,
                    ApplicationUserId = r.ApplicationUserId,
                    NumberOfGuests = r.NumberOfGuests,
                    Notes = r.Notes,
                    ReservationStart = r.ReservationStart,
                    ReservationEnd = r.ReservationEnd,
                    Status = r.Status
                })
                .ToListAsync(cancellationToken);

            return reservations;
        }
    }
}
