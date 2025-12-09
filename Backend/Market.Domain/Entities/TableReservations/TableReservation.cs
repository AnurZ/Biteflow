using Market.Domain.Common;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.Identity;
using Market.Domain.Entities.IdentityV2;
using System;

namespace Market.Domain.Entities.TableReservations
{
    public class TableReservation : BaseEntity
    {
        // Relationships
        public int DiningTableId { get; set; }
        public DiningTable? DiningTable { get; set; }  // nullable if lazy-loaded or optional

        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        // Customer info
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Reservation details
        public int NumberOfGuests { get; set; }
        public string? Notes { get; set; }  

        public DateTime ReservationStart { get; set; }
        public DateTime? ReservationEnd { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public bool IsActive => Status == ReservationStatus.Confirmed &&
                                ReservationEnd is null || ReservationEnd > DateTime.UtcNow;

        public TimeSpan? Duration => ReservationEnd.HasValue
                                     ? ReservationEnd - ReservationStart
                                     : null;
    }
}
