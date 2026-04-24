using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Analytics.Queries.GetRevenuePerDay
{
    public sealed class GetRevenuePerDayQuery : IRequest<List<GetRevenuePerDayDto>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
