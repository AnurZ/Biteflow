using Market.Domain.Common.Enums;

namespace Market.Application.Modules.Orders.Queries.AdminGetOrders
{
    public sealed class AdminGetOrdersQuery : BasePagedQuery<AdminOrderDto>
    {
        public List<OrderStatus>? Statuses { get; set; }
        public string? Sort { get; init; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
    }
}