using Market.Domain.Common;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.Identity;
using Market.Domain.Entities.IdentityV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.TableReservations
{
    public class TableReservation : BaseEntity
    {
        public int DiningTableId { get; set; }
        public DiningTable DiningTable { get; set; }

        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int NumberOfGuests { get; set; }
        public string Notes { get; set; }

        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }


}
