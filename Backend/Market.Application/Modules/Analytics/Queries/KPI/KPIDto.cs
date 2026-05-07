using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Analytics.Queries.KPI
{
    public sealed class KpiDto
    {
        public int TotalOrders { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public string TopItem { get; set; }
    }
}
