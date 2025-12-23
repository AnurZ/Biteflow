using Market.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace Market.Application.Modules.TableReservation.Queries.GetTableReservations
{
    public sealed class GetTableReservationsQuery : IRequest<List<GetTableReservationsQueryDto>>
    {
        public int? ReservationId { get; set; }
        public int? DiningTableId { get; set; }
        public DateTime? RequestedStart { get; set; }
        public DateTime? RequestedEnd { get; set; }
        public ReservationStatus? Status { get; set; } // optional filter by status
    }

    public sealed class GetTableReservationsQueryDto
    {
        public int Id { get; set; }
        public int DiningTableId { get; set; }
        public int DiningTableNumber { get; set; }
        public int TableLayoutId { get; set; }
        public string TableLayoutName { get; set; }
        public Guid? ApplicationUserId { get; set; } // nullable, matches entity
        public string FirstName { get; set; } = string.Empty; // include customer info
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int NumberOfGuests { get; set; }
        public string? Notes { get; set; } // optional notes
        public DateTime ReservationStart { get; set; }
        public DateTime? ReservationEnd { get; set; } // nullable
        public ReservationStatus Status { get; set; }
    }
}
