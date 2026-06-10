using Market.Application.Modules.Orders;
using Market.Domain.Common.Enums;

namespace Market.Application.Modules.Orders.Commands.CreateOrder;

public sealed record CreateOrderResult(
    int Id,
    Guid TenantId,
    int? TableNumber,
    string? Notes,
    DateTime CreatedAtUtc,
    OrderStatus Status,
    IReadOnlyCollection<OrderRealtimeItemResult> Items,
    OrderNotificationResult CreatedNotification);
