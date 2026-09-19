using Market.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.TableReservation.Queries.GetTableReservations
{
    public sealed class GetTableReservationsQueryHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
        : IRequestHandler<GetTableReservationsQuery, List<GetTableReservationsQueryDto>>
    {
        public async Task<List<GetTableReservationsQueryDto>> Handle(
            GetTableReservationsQuery request,
            CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var query = db.TableReservations
                .Where(r =>
                    r.DiningTable != null &&
                    r.DiningTable.TableLayout.RestaurantId == restaurantId)
                .AsQueryable();

            if (request.ReservationId.HasValue)
            {
                query = query.Where(r => r.Id == request.ReservationId.Value);
            }

            if (request.DiningTableId.HasValue)
            {
                query = query.Where(r => r.DiningTableId == request.DiningTableId.Value);
            }

            if (request.RequestedStart.HasValue &&
                request.RequestedEnd.HasValue)
            {
                var start = request.RequestedStart.Value;
                var end = request.RequestedEnd.Value;
                var buffer = TimeSpan.FromMinutes(90);

                query = query.Where(r =>
                    r.Status != ReservationStatus.Cancelled &&
                    r.ReservationStart < end.Add(buffer) &&
                    (r.ReservationEnd ?? DateTime.MaxValue) > start);
            }

            return await query
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
        }
    }
}