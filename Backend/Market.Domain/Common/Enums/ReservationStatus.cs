using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Common.Enums
{
    public enum ReservationStatus
    {
        Pending = 0,     // Before the guest arrives
        Confirmed = 1,   // Staff confirmed it
        Completed = 2,   // Guest came and finished
        Cancelled = 3,
        NoShow = 4
    }

}
