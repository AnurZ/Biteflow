using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Analytics.Queries.GetRevenuePerDay
{
    public sealed class GetRevenuePerDayDto
    {
        public string Date { get; set; } = default!;
        public decimal Revenue { get; set; }
    }
}
