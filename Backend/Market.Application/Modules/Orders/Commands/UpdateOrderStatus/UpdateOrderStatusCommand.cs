using Market.Domain.Common.Enums;
using MediatR;

namespace Market.Application.Modules.Orders.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommand : IRequest
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
    }
}
