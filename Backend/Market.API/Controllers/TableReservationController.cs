using MediatR;
using Microsoft.AspNetCore.Mvc;
using Market.Application.Modules.TableReservation.Commands.CreateTableReservation;
using Market.Application.Modules.TableReservation.Queries.GetTableReservations;

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
    }
}
