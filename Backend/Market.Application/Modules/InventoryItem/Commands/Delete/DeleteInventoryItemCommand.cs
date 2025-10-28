using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Commands.Delete
{
    public sealed class DeleteInventoryItemCommand : IRequest
    {
        public int Id { get; init; }
    }
}
