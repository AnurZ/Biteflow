using Market.Application.Modules.Orders.Commands.CreateOrder;
using Market.Application.Modules.Orders.Commands.UpdateOrderStatus;
using Market.Application.Modules.Orders.Queries.GetOrders;
using Market.Domain.Common.Enums;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
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
            return Created(string.Empty, new { id });
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Policy = PolicyNames.StaffMember)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusCommand command, CancellationToken ct)
        {
            command.Id = id;
            await _sender.Send(command, ct);
            return NoContent();
        }
    }
}
