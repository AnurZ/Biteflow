using MediatR;

namespace Market.Application.Modules.TableReservation.Commands.CreateTableReservation
{
    public sealed class CreatePublicTableReservationCommandDto : IRequest<int>
    {
        public int DiningTableId { get; set; }
        public int NumberOfGuests { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime? ReservationEnd { get; set; }
        public string? Notes { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
