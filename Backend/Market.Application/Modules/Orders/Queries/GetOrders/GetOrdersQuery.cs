using Market.Domain.Common.Enums;
using MediatR;

namespace Market.Application.Modules.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQuery : IRequest<List<OrderDto>>
    {
        public List<OrderStatus>? Statuses { get; set; }
    }
}
