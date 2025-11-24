using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.DeleteTableReservation
{
    public sealed class DeleteTableReservationCommandDto:IRequest
    {
        public int Id { get; set; }
    }
}
