using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Application.Modules.TableLayout.Commands.CreateTableLayout;
using Market.Application.Modules.TableLayout.Commands.UpdateTableLayout;
using Market.Application.Modules.TableLayout.Commands.DeleteTableLayout;
using Market.Application.Modules.TableLayout.Querries.GetTableLayouts;
using Market.Application.Modules.TableLayout.Queries.TableLayoutGetNameById;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableLayoutController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TableLayoutController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/TableLayout
        [HttpGet]
        public async Task<ActionResult<List<TableLayoutDto>>> Get([FromQuery] GetTableLayoutsQuery query)
        {
            var layouts = await _mediator.Send(query);
            return Ok(layouts);
        }

        [HttpGet("{id:int}/name")]
        public async Task<TableLayoutGetNameByIdDto> GetNameById(int id)
        {
            return await _mediator.Send(new TableLayoutGetNameByIdQuery(id));
        }


        // POST: api/TableLayout
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateTableLayoutCommandDto command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id }, id);
        }

        // PUT: api/TableLayout/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTableLayoutCommandDto cmd)
        {
            cmd.Id = id;  // overwrite DTO.Id with route value
            await _mediator.Send(cmd);
            return NoContent();
        }

        // DELETE: api/TableLayout/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteTableLayoutCommandDto { Id = id });
            return NoContent();
        }
    }
}
