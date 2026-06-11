using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Orders.Queries.GetOrderById
{
    public sealed class OrderByIdDto
    {
        public int Id { get; set; }
        public int? TableLayoutId { get; set; }
        public string? TableLayoutName { get; set; }
        public int? DiningTableId { get; set; }
        public int? TableNumber { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? Notes { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderByIdItemDto> Items { get; set; } = new();
    }

    public sealed class OrderByIdItemDto
    {
        public int Id { get; set; }
        public int? MealId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
