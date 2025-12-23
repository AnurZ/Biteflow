using Market.Domain.Common;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using System.Collections.Generic;

namespace Market.Domain.Entities.Orders
{
    public class Order : BaseEntity
    {
        public int? DiningTableId { get; set; }  // nullable eat-out order
        public DiningTable? DiningTable { get; set; }
        public int? TableNumber { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string? Notes { get; set; }
    }
}
