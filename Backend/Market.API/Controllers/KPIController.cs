using MediatR;
using Microsoft.AspNetCore.Mvc;
using Market.Application.Modules.Analytics.Queries.KPI;

namespace Market.API.Controllers.Analytics
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("kpis")]
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