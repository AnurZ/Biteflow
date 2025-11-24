using Market.Domain.Common.Enums;
using System;

namespace Market.Application.Modules.TableReservation.Queries.GetTableReservations
{
    public sealed class GetTableReservationsQuery : IRequest<List<GetTableReservationsQueryDto>>
    {
        public int? ReservationId { get; set; }
        public int? DiningTableId { get; set; }
        public DateTime? RequestedStart { get; set; }
        public DateTime? RequestedEnd { get; set; }
    }
    public sealed class GetTableReservationsQueryDto
    {
        public int Id { get; set; }
        public int DiningTableId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public int NumberOfGuests { get; set; }
        public string Notes { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
