using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation
{
    public sealed class UpdateTableReservationCommandDto : IRequest
    {
        public int Id { get; set; }
        public int DiningTableId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public int NumberOfGuests { get; set; }
        public string Notes { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
    }
}
