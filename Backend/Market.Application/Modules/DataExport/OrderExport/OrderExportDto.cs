using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.DataExport.OrderExport
{
    public sealed class OrderExportDto
    {
        public int OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ItemCount { get; set; }
        public int? DiningTableId { get; set; }

        public int? TableNumber { get; set; }
    }
}
