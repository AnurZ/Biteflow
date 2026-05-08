using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Queries.GetById
{
    public sealed class GetInventoryItemByIdQuery : IRequest<GetInventoryItemByIdDto>
    {
        public int Id { get; init; }
    }
}
