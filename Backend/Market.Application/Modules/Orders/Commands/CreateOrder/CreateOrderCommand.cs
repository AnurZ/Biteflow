using Market.Domain.Common.Enums;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Market.Application.Modules.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommand : IRequest<int>
    {
        public int? DiningTableId { get; set; }
        public int? TableNumber { get; set; }
        public string? Notes { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public sealed class CreateOrderItemDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        public int? MealId { get; set; }
    }
}
