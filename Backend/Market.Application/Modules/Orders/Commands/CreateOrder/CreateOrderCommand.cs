using Market.Domain.Common.Enums;
using MediatR;

namespace Market.Application.Modules.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommand : IRequest<int>
    {
        public int? DiningTableId { get; set; }
        [JsonIgnore]
        public int? TableNumber { get; set; }
        public string? Notes { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public sealed class CreateOrderItemDto
    {
        public bool IsCustom { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? MealId { get; set; }
    }
}
