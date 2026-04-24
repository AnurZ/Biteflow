using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Analytics.Queries.GetTopSellingItems
{
    public sealed class GetTopSellingItemsQuery : IRequest<List<GetTopSellingItemsDto>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
