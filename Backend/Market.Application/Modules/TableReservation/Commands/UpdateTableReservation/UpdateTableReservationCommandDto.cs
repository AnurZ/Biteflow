using MediatR;
using Market.Domain.Common.Enums;
using System;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation
{
    public sealed class UpdateTableReservationCommandDto : IRequest
    {
        // Required
        public int Id { get; set; }
        public int DiningTableId { get; set; }
        public int NumberOfGuests { get; set; }
        public DateTime ReservationStart { get; set; }

        // Optional
        public Guid? ApplicationUserId { get; set; } // nullable if guest doesn't have an account
        public DateTime? ReservationEnd { get; set; } // nullable end time
        public string? Notes { get; set; } // optional notes

        // Optional customer info for guests without accounts
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
