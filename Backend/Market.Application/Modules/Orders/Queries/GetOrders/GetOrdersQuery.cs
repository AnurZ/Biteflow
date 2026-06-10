using Market.Application.Common;
using Market.Domain.Common.Enums;

namespace Market.Application.Modules.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQuery : BasePagedQuery<OrderDto>
    {
        public List<OrderStatus>? Statuses { get; set; }
    }
}
