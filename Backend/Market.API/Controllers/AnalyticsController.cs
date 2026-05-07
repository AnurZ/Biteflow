using Microsoft.AspNetCore.Mvc;
using MediatR;
using Market.Application.Modules.Analytics.Queries.GetOrdersPerDay;
using Market.Application.Modules.Analytics.Queries.GetTopSellingItems;
using Market.Application.Modules.Analytics.Queries.GetRevenuePerDay;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PolicyNames.StaffMember)]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("orders-per-day")]
        public async Task<IActionResult> GetOrdersPerDay(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _mediator.Send(new GetOrdersPerDayQuery
            {
                From = from,
                To = to
            });

            return Ok(result);
        }

        [HttpGet("top-selling-items")]
        public async Task<IActionResult> GetTopSellingItems([FromQuery] GetTopSellingItemsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("revenue-per-day")]
        public async Task<IActionResult> GetRevenuePerDay([FromQuery] GetRevenuePerDayQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
