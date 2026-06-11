using Market.Application.Modules.Orders;
using Market.Domain.Common.Enums;

namespace Market.Application.Modules.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusResult(
    int OrderId,
    Guid TenantId,
    OrderStatus Status,
    bool StatusChanged,
    OrderNotificationResult? CreatedNotification,
    IReadOnlyCollection<int> ClearedNotificationIds);
