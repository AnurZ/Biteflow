using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Analytics.Queries.GetOrdersPerDay
{
    public sealed class GetOrdersPerDayDto
    {
        public string Date { get; set; } = default!;
        public int Count { get; set; }
    }
}
