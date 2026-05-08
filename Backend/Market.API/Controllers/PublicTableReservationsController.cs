using Market.Application.Modules.TableReservation.Commands.CreateTableReservation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

[ApiController]
[Route("api/public/table-reservations")]
[AllowAnonymous]
public sealed class PublicTableReservationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> CreateReservation(
        [FromBody] CreatePublicTableReservationCommandDto request,
        CancellationToken ct)
    {
        var id = await mediator.Send(request, ct);
        return Ok(id);
    }
}
