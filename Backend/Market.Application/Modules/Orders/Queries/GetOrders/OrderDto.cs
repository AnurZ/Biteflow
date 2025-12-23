using Market.Domain.Common.Enums;

namespace Market.Application.Modules.Orders.Queries.GetOrders
{
    public sealed class OrderDto
    {
        public int Id { get; set; }
        public int? DiningTableId { get; set; }
        public int? TableNumber { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public sealed class OrderItemDto
    {
        public int Id { get; set; }
        public int? MealId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
