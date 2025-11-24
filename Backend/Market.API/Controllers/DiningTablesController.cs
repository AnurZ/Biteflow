using MediatR;
using Microsoft.AspNetCore.Mvc;
using Market.Application.Modules.DiningTable.Commands.CreateDiningTable;
using Market.Application.Modules.DiningTable.Querries.GetDiningTableList;
using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Domain.Common.Enums;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiningTableController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DiningTableController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new dining table
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDiningTable([FromBody] CreateDiningTableCommandDto command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get a list of dining tables with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GetDiningTableListQueryDto>>> GetDiningTables(
            [FromQuery] string? sectionName,
            [FromQuery] TableStatus? status,
            [FromQuery] int? minimumSeats)
        {
            var query = new GetDiningTableListQuery
            {
                SectionName = sectionName,
                Status = status,
                MinimumSeats = minimumSeats
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
