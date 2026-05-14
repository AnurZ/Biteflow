using MediatR;
using Microsoft.AspNetCore.Mvc;
using Market.Application.Modules.Analytics.Queries.KPI;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Market.API.Controllers.Analytics
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize(Policy = PolicyNames.RestaurantAdmin)]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("kpis")]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<ActionResult<KpiDto>> GetKpis(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _mediator.Send(new GetKpisQuery
            {
                From = from,
                To = to
            });

            return Ok(result);
        }
    }
}
