using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.UpdateTableReservation.UpdateTableReservationStatus
{
    public sealed class UpdateTableReservationStatusDto : IRequest
    {
        public int Id { get; set; }
        public int Status { get; set; }
    }

}
