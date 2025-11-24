using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.CreateTableReservation
{
    public sealed class CreateTableReservationCommandDto:IRequest<int>
    {
        public int DiningTableId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public int NumberOfGuests { get; set; }
        public string Notes { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
    }
}
