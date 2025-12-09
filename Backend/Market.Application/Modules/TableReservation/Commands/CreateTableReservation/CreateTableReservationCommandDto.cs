using MediatR;
using System;

namespace Market.Application.Modules.TableReservation.Commands.CreateTableReservation
{
    public sealed class CreateTableReservationCommandDto : IRequest<int>
    {
        // Required
        public int DiningTableId { get; set; }
        public int NumberOfGuests { get; set; }
        public DateTime ReservationStart { get; set; }

        // Optional
        public Guid? ApplicationUserId { get; set; } // nullable if guest doesn't have account
        public DateTime? ReservationEnd { get; set; } // nullable end time
        public string? Notes { get; set; } // optional notes

        // Optional customer info for guest reservations
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
