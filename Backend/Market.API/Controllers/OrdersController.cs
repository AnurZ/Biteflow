using System;
using System.Linq;
using Market.API.Hubs;
using Market.Application.Modules.Orders;
using Market.Application.Modules.Orders.Commands.CreateOrder;
using Market.Application.Modules.Orders.Commands.UpdateOrderStatus;
using Market.Application.Modules.Orders.Queries.AdminGetOrders;
using Market.Application.Modules.Orders.Queries.GetOrderById;
using Market.Application.Modules.Orders.Queries.GetOrders;
using Market.Domain.Common.Enums;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IHubContext<OrdersHub> _hub;

        public OrdersController(
            ISender sender,
            IHubContext<OrdersHub> hub)
        {
            _sender = sender;
            _hub = hub;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<PageResult<OrderDto>> GetOrders(
            [FromQuery] OrderStatus[]? statuses,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var query = new GetOrdersQuery
            {
                Statuses = statuses?.ToList() ?? new List<OrderStatus>(),
                Paging = new PageRequest { Page = page, PageSize = pageSize }
            };

            return await _sender.Send(query, ct);
        }

        [HttpGet("admin")]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<ActionResult<PageResult<AdminOrderDto>>> AdminGetOrders(
        [FromQuery] AdminGetOrdersQuery query,
        CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<ActionResult<OrderByIdDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _sender.Send(new GetOrderByIdQuery { Id = id }, ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<ActionResult<int>> Create([FromBody] CreateOrderCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);

            await _hub.Clients
                .Group(OrdersHubGroups.Kitchen(result.TenantId))
                .SendAsync(OrdersHubEvents.OrderCreated, new
                {
                    orderId = result.Id,
                    tableNumber = result.TableNumber,
                    note = result.Notes,
                    createdAt = result.CreatedAtUtc,
                    status = result.Status.ToString()
                }, ct);

            await SendNotificationCreatedAsync(result.CreatedNotification, result.TenantId, ct);

            await _hub.Clients
                .Group(OrdersHubGroups.Admin(result.TenantId))
                .SendAsync(OrdersHubEvents.OrderCreated, new
                {
                    orderId = result.Id,
                    tableNumber = result.TableNumber,
                    status = result.Status.ToString(),
                    createdAt = result.CreatedAtUtc,
                    items = result.Items.Select(i => new
                    {
                        i.Name,
                        i.Quantity
                    })
                }, ct);

            await _hub.Clients
                .Group(OrdersHubGroups.Admin(result.TenantId))
                .SendAsync(OrdersHubEvents.DashboardUpdated, new
                {
                    type = "order_created",
                    orderId = result.Id
                }, ct);

            return Created(string.Empty, new { id = result.Id });
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusCommand command, CancellationToken ct)
        {
            command.Id = id;
            var result = await _sender.Send(command, ct);

            if (result.StatusChanged)
            {
                var payload = new
                {
                    orderId = result.OrderId,
                    status = result.Status.ToString()
                };

                var waiterGroup = OrdersHubGroups.Waiter(result.TenantId);
                var kitchenGroup = OrdersHubGroups.Kitchen(result.TenantId);
                var adminGroup = OrdersHubGroups.Admin(result.TenantId);

                await _hub.Clients
                    .Groups(waiterGroup, kitchenGroup, adminGroup)
                    .SendAsync(OrdersHubEvents.OrderStatusChanged, payload, ct);

                await _hub.Clients
                    .Group(adminGroup)
                    .SendAsync(OrdersHubEvents.DashboardUpdated, new
                    {
                        type = "order_status_changed",
                        orderId = result.OrderId,
                        status = result.Status.ToString()
                    }, ct);

                if (result.CreatedNotification != null)
                {
                    await SendNotificationCreatedAsync(result.CreatedNotification, result.TenantId, ct);
                }

                if (result.Status == OrderStatus.Completed)
                {
                    var waiterRole = OrdersHubGroups.Role(RoleNames.Waiter, result.TenantId);
                    var kitchenRole = OrdersHubGroups.Role(RoleNames.Kitchen, result.TenantId);

                    var groups = new[] { waiterRole, kitchenRole }
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .ToArray();

                    if (groups.Length > 0)
                    {
                        await _hub.Clients
                            .Groups(groups)
                            .SendAsync(OrdersHubEvents.NotificationCleared, new
                            {
                                orderId = result.OrderId,
                                notificationIds = result.ClearedNotificationIds.ToArray()
                            }, ct);
                    }
                }
            }

            return NoContent();
        }

        private async Task SendNotificationCreatedAsync(OrderNotificationResult notification, Guid tenantId, CancellationToken ct)
        {
            var roleGroup = OrdersHubGroups.Role(notification.TargetRole ?? string.Empty, tenantId);
            if (string.IsNullOrWhiteSpace(roleGroup))
            {
                return;
            }

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
}
