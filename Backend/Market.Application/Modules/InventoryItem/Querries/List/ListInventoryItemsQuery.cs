using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Application.Modules.InventoryItem.Querries.List;
using MediatR;

namespace Market.Application.Modules.InventoryItem.Queries.List
{
    public sealed class ListInventoryItemsQuery : BasePagedQuery<ListInventoryItemsDto>
    {
        public string? Search { get; init; }
        public string? Sort { get; init; }
    }
}
