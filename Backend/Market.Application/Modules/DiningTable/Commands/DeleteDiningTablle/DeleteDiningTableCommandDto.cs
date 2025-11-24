using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Commands.DeleteDiningTablle
{
    public sealed class DeleteDiningTableCommandDto:IRequest
    {
        public int Id { get; set; }
    }
}
