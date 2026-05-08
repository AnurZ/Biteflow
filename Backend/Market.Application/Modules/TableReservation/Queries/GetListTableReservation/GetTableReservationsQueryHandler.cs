using MediatR;
using Microsoft.EntityFrameworkCore;
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

            // Filter by ReservationId
            if (request.ReservationId.HasValue)
                query = query.Where(r => r.Id == request.ReservationId.Value);

            // Filter by DiningTableId
            if (request.DiningTableId.HasValue)
                query = query.Where(r => r.DiningTableId == request.DiningTableId.Value);

            // Filter by Status
            if (request.Status.HasValue)
                query = query.Where(r => r.Status == request.Status.Value);

            if (request.RequestedStart.HasValue && request.RequestedEnd.HasValue)
            {
                var start = request.RequestedStart.Value;
                var end = request.RequestedEnd.Value;

                Console.WriteLine(start);
                Console.WriteLine(end);
                query = query.Where(r =>
                    r.ReservationEnd.HasValue
                        ? r.ReservationStart < end && r.ReservationEnd > start
                        : r.ReservationStart < end);
            }






            var reservations = await query
                .Select(r => new GetTableReservationsQueryDto
                {
                    Id = r.Id,
                    DiningTableId = r.DiningTableId,
                    DiningTableNumber = r.DiningTable.Number,
                    ApplicationUserId = r.ApplicationUserId,

                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Email = r.Email,
                    PhoneNumber = r.PhoneNumber,

                    NumberOfGuests = r.NumberOfGuests,
                    Notes = r.Notes,

                    ReservationStart = r.ReservationStart,
                    ReservationEnd = r.ReservationEnd,
                    Status = r.Status,

                    TableLayoutName = r.DiningTable.TableLayout.Name,
                    TableLayoutId = r.DiningTable.TableLayoutId
                })
                .ToListAsync(cancellationToken);


            return reservations;
        }
    }
}
