using Market.Application.Abstractions;
using Market.Application.Common.Exceptions;
using Market.Application.Modules.Orders;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Market.Shared.Constants;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Market.Application.Modules.Orders.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
    {
        private const string OrderCreatedNotificationType = "OrderCreated";
        private const string OrderReadyNotificationType = "OrderReady";

        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
        {
            { OrderStatus.New, new[] { OrderStatus.Cooking, OrderStatus.Cancelled } },
            { OrderStatus.Cooking, new[] { OrderStatus.ReadyForPickup, OrderStatus.Cancelled } },
            { OrderStatus.ReadyForPickup, new[] { OrderStatus.Completed, OrderStatus.Cancelled } },
            { OrderStatus.Completed, Array.Empty<OrderStatus>() },
            { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
        };

        private readonly IAppDbContext _db;
        private readonly IAppCurrentUser _currentUser;

        public UpdateOrderStatusCommandHandler(IAppDbContext db, IAppCurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with id {request.Id} not found.");
            }

            var current = order.Status;
            if (IsTerminalNoOp(current, request.Status))
            {
                return new UpdateOrderStatusResult(
                    order.Id,
                    order.TenantId,
                    order.Status,
                    false,
                    null,
                    Array.Empty<int>());
            }

            EnsureValidTransition(current, request.Status);
            EnsureRoleCanTransition(current, request.Status);

            order.Status = request.Status;
            await _db.SaveChangesAsync(cancellationToken);

            OrderNotificationResult? createdNotification = null;
            var clearedNotificationIds = Array.Empty<int>();

            if (order.Status == OrderStatus.ReadyForPickup)
            {
                var notification = new NotificationEntity
                {
                    TenantId = order.TenantId,
                    OrderId = order.Id,
                    TargetRole = RoleNames.Waiter,
                    Title = "Narudzba spremna",
                    Message = $"Sto {order.TableNumber ?? order.DiningTableId} - narudzba je spremna.",
                    Type = OrderReadyNotificationType,
                    Link = $"/waiter/orders/{order.Id}"
                };

                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync(cancellationToken);
                createdNotification = ToNotificationResult(notification);
            }

            if (order.Status == OrderStatus.Completed)
            {
                var notificationTypes = new[] { OrderCreatedNotificationType, OrderReadyNotificationType };
                var notifications = await _db.Notifications
                    .Where(n => n.TenantId == order.TenantId &&
                                n.OrderId == order.Id &&
                                notificationTypes.Contains(n.Type))
                    .ToListAsync(cancellationToken);

                clearedNotificationIds = notifications
                    .Select(n => n.Id)
                    .ToArray();

                if (notifications.Count > 0)
                {
                    var now = DateTime.UtcNow;

                    foreach (var item in notifications)
                    {
                        item.ReadAtUtc ??= now;
                    }

                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            return new UpdateOrderStatusResult(
                order.Id,
                order.TenantId,
                order.Status,
                true,
                createdNotification,
                clearedNotificationIds);
        }

        private static bool IsTerminalNoOp(OrderStatus current, OrderStatus requested)
            => current == requested && current is OrderStatus.Completed or OrderStatus.Cancelled;

        private static void EnsureValidTransition(OrderStatus current, OrderStatus requested)
        {
            if (AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Length > 0)
            {
                if (!allowed.Contains(requested))
                {
                    throw new ValidationException($"Cannot change order status from {current} to {requested}.");
                }

                return;
            }

            if (current is OrderStatus.Completed or OrderStatus.Cancelled)
            {
                throw new ValidationException("Completed or cancelled orders cannot change status.");
            }

            throw new ValidationException($"Cannot change order status from {current} to {requested}.");
        }

        private void EnsureRoleCanTransition(OrderStatus current, OrderStatus requested)
        {
            if (_currentUser.IsInRole(RoleNames.Admin))
            {
                return;
            }

            if (_currentUser.IsInRole(RoleNames.Kitchen) &&
                ((current == OrderStatus.New && requested == OrderStatus.Cooking) ||
                 (current == OrderStatus.Cooking && requested == OrderStatus.ReadyForPickup)))
            {
                return;
            }

            if (_currentUser.IsInRole(RoleNames.Waiter) &&
                current == OrderStatus.ReadyForPickup &&
                requested == OrderStatus.Completed)
            {
                return;
            }

            throw new MarketForbiddenException($"Current role cannot change order status from {current} to {requested}.");
        }

        private static OrderNotificationResult ToNotificationResult(NotificationEntity notification)
            => new(
                notification.Id,
                notification.TargetRole,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Link,
                notification.CreatedAtUtc,
                notification.ReadAtUtc);
    }
}
