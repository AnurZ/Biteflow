using Market.API.Hubs;
using Market.Application.Abstractions;
using Market.Application.Modules.Orders.Commands.CreateOrder;
using Market.Application.Modules.Orders.Commands.UpdateOrderStatus;
using Market.Application.Modules.Orders.Queries.GetOrders;
using Market.Domain.Common.Enums;
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
            }
            return NoContent();
        }
    }
}
