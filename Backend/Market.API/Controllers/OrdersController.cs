using System;
using System.Linq;
using Market.API.Hubs;
using Market.Application.Abstractions;
using Market.Application.Modules.Orders.Commands.CreateOrder;
using Market.Application.Modules.Orders.Commands.UpdateOrderStatus;
using Market.Application.Modules.Orders.Queries.GetOrders;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Notifications;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IAppDbContext _db;
        private readonly IHubContext<OrdersHub> _hub;

        public OrdersController(ISender sender, IAppDbContext db, IHubContext<OrdersHub> hub)
        {
            _sender = sender;
            _db = db;
            _hub = hub;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<List<OrderDto>> GetOrders([FromQuery] OrderStatus[]? statuses, CancellationToken ct)
        {
            var query = new GetOrdersQuery
            {
                Statuses = statuses?.ToList() ?? new List<OrderStatus>()
            };

            return await _sender.Send(query, ct);
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<ActionResult<int>> Create([FromBody] CreateOrderCommand command, CancellationToken ct)
        {
            var id = await _sender.Send(command, ct);
            var order = await _db.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (order != null)
            {
                var notification = new NotificationEntity
                {
                    TenantId = order.TenantId,
                    TargetRole = "Kitchen",
                    Title = "Nova narudzba",
                    Message = $"Sto {order.TableNumber ?? order.DiningTableId} - nova narudzba je stigla.",
                    Type = "OrderCreated",
                    Link = $"/kitchen/orders/{order.Id}"
                };

                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync(ct);

                var payload = new
                {
                    orderId = order.Id,
                    tableNumber = order.TableNumber,
                    note = order.Notes,
                    createdAt = order.CreatedAtUtc,
                    status = order.Status.ToString()
                };

                await _hub.Clients
                    .Group(OrdersHubGroups.Kitchen(order.TenantId))
                    .SendAsync(OrdersHubEvents.OrderCreated, payload, ct);

                var roleGroup = OrdersHubGroups.Role(notification.TargetRole ?? string.Empty, order.TenantId);
                if (!string.IsNullOrWhiteSpace(roleGroup))
                {
                    await _hub.Clients
                        .Group(roleGroup)
                        .SendAsync(OrdersHubEvents.NotificationCreated, new
                        {
                            id = notification.Id,
                            title = notification.Title,
                            message = notification.Message,
                            type = notification.Type,
                            link = notification.Link,
                            createdAtUtc = notification.CreatedAtUtc,
                            readAtUtc = notification.ReadAtUtc
                        }, ct);
                }
            }
            return Created(string.Empty, new { id });
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusCommand command, CancellationToken ct)
        {
            command.Id = id;
            await _sender.Send(command, ct);

            var order = await _db.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (order != null)
            {
                var payload = new
                {
                    orderId = order.Id,
                    status = order.Status.ToString()
                };

                var waiterGroup = OrdersHubGroups.Waiter(order.TenantId);
                var kitchenGroup = OrdersHubGroups.Kitchen(order.TenantId);

                await _hub.Clients
                    .Groups(waiterGroup, kitchenGroup)
                    .SendAsync(OrdersHubEvents.OrderStatusChanged, payload, ct);

                if (order.Status == OrderStatus.ReadyForPickup)
                {
                    var notification = new NotificationEntity
                    {
                        TenantId = order.TenantId,
                        TargetRole = "Waiter",
                        Title = "Narudzba spremna",
                        Message = $"Sto {order.TableNumber ?? order.DiningTableId} - narudzba je spremna.",
                        Type = "OrderReady",
                        Link = $"/waiter/orders/{order.Id}"
                    };

                    _db.Notifications.Add(notification);
                    await _db.SaveChangesAsync(ct);

                    var roleGroup = OrdersHubGroups.Role(notification.TargetRole ?? string.Empty, order.TenantId);
                    if (!string.IsNullOrWhiteSpace(roleGroup))
                    {
                        await _hub.Clients
                            .Group(roleGroup)
                            .SendAsync(OrdersHubEvents.NotificationCreated, new
                            {
                                id = notification.Id,
                                title = notification.Title,
                                message = notification.Message,
                                type = notification.Type,
                                link = notification.Link,
                                createdAtUtc = notification.CreatedAtUtc,
                                readAtUtc = notification.ReadAtUtc
                            }, ct);
                    }
                }

                if (order.Status == OrderStatus.Completed)
                {
                    var linkSuffix = $"/{order.Id}";
                    var notifications = await _db.Notifications
                        .Where(n => n.TenantId == order.TenantId &&
                                    n.Link != null &&
                                    n.Link.EndsWith(linkSuffix))
                        .ToListAsync(ct);

                    if (notifications.Count > 0)
                    {
                        var now = DateTime.UtcNow;
                        foreach (var item in notifications)
                        {
                            item.ReadAtUtc ??= now;
                        }

                        await _db.SaveChangesAsync(ct);
                    }

                    var waiterRole = OrdersHubGroups.Role("Waiter", order.TenantId);
                    var kitchenRole = OrdersHubGroups.Role("Kitchen", order.TenantId);
                    var groups = new[] { waiterRole, kitchenRole }
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .ToArray();

                    if (groups.Length > 0)
                    {
                        await _hub.Clients
                            .Groups(groups)
                            .SendAsync(OrdersHubEvents.NotificationCleared, new
                            {
                                orderId = order.Id,
                                notificationIds = notifications.Select(n => n.Id).ToArray()
                            }, ct);
                    }
                }
            }
            return NoContent();
        }
    }
}
