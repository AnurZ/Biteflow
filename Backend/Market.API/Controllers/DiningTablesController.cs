using MediatR;
using Microsoft.AspNetCore.Mvc;
using Market.Application.Modules.DiningTable.Commands.CreateDiningTable;
using Market.Application.Modules.DiningTable.Commands.UpdateDiningTable;
using Market.Application.Modules.DiningTable.Querries.GetDiningTableList;
using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Domain.Common.Enums;
using Market.Application.Modules.DiningTable.Commands.DeleteDiningTablle;
using Market.Application.Modules.DiningTable.Querries.GetDiningTableTLIDbyTableID;
using Market.Application.Modules.DiningTable.Querries.GetDiningTableStatus;

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
        /// Update an existing dining table
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDiningTable(int id, [FromBody] UpdateDiningTableCommandDto command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Delete a dining table
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDiningTable(int id)
        {
            var command = new DeleteDiningTableCommandDto { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Get a list of dining tables with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GetDiningTableListQueryDto>>> GetDiningTables(
            [FromQuery] string? tableLayoutName,
            [FromQuery] TableStatus? status,
            [FromQuery] int? minimumSeats,
            [FromQuery] int? tableLayoutId,
            [FromQuery] int? id,
            [FromQuery] int? number) 
        {
            var query = new GetDiningTableListQuery
            {
                TableLayoutName = tableLayoutName,
                Status = status,
                MinimumSeats = minimumSeats,
                TableLayoutId = tableLayoutId,
                Id = id,
                Number = number
            };
        
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("dining-table/{id}/table-layout-id")]
        public async Task<ActionResult<GetDiningTableTableLayoutIdByIdDto>> GetTableLayoutId(int id)
        {
            var result = await _mediator.Send(new GetDiningTableTableLayoutIdByIdQuery
            {
                DiningTableId = id
            });

            return Ok(result);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetAll([FromQuery] int? tableLayoutId)
        {
            var query = new GetDiningTablesStatusQuery
            {
                TableLayoutId = tableLayoutId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }


    }
}
