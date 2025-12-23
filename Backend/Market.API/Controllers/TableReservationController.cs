using MediatR;
using Microsoft.AspNetCore.Mvc;
using Market.Application.Modules.TableReservation.Commands.CreateTableReservation;
using Market.Application.Modules.TableReservation.Queries.GetTableReservations;
using Market.Application.Modules.TableReservation.Commands.DeleteTableReservation;
using Market.Shared.Constants;
using Market.Application.Modules.TableReservation.Commands.UpdateTableReservation;
using Market.Application.Modules.TableReservation.Commands.UpdateTableReservation.UpdateTableReservationStatus;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableReservationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TableReservationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateReservation([FromBody] CreateTableReservationCommandDto request)
        {
            try
            {
                var id = await _mediator.Send(request);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // This will catch conflicts or invalid data
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<GetTableReservationsQueryDto>>> GetReservations(
            [FromQuery] int? reservationId,
            [FromQuery] int? diningTableId,
            [FromQuery] DateTime? requestedStart,
            [FromQuery] DateTime? requestedEnd)
        {
            var query = new GetTableReservationsQuery
            {
                ReservationId = reservationId,
                DiningTableId = diningTableId,
                RequestedStart = requestedStart,
                RequestedEnd = requestedEnd
            };

            var reservations = await _mediator.Send(query);
            return Ok(reservations);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteTableReservationCommandDto { Id = id }, ct);
            return NoContent();
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<IActionResult> Update(int id, UpdateTableReservationCommandDto cmd, CancellationToken ct)
        {
            cmd.Id = id;
            await _mediator.Send(cmd, ct);
            return NoContent();
        }

        [HttpPatch("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateTableReservationStatusDto dto)
        {
            await _mediator.Send(dto);
            return NoContent();
        }

    }
}
