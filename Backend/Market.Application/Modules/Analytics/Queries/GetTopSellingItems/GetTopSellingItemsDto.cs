using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Analytics.Queries.GetTopSellingItems
{
    public sealed class GetTopSellingItemsDto
    {
        public string ItemName { get; set; } = default!;
        public int Quantity { get; set; }
    }
}
