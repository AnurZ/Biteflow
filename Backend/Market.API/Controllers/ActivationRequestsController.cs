using Market.Application.Abstractions;
using Market.Application.Common;
using Market.Application.Modules.TenantActivation.Commands.ApproveRequest;
using Market.Application.Modules.TenantActivation.Commands.ConfirmActivation;
using Market.Application.Modules.TenantActivation.Commands.Create;
using Market.Application.Modules.TenantActivation.Commands.RejectRequest;
using Market.Application.Modules.TenantActivation.Queries.List;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Tenants;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

[ApiController]
[Route("api/activation-requests")]
public sealed class ActivationRequestsController(IMediator mediator) : ControllerBase
{

    public sealed record RejectDto(string Reason);
    public sealed record ConfirmDto(string Token);



    // Submit activation request
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDraftCommand cmd)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        await mediator.Send(cmd);
        return NoContent();
    }



    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    [HttpGet]
    [ProducesResponseType(typeof(PageResult<ActivationDraftDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResult<ActivationDraftDto>>> List(
        [FromQuery] ActivationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new ListRequestsQuery(status, page, pageSize));
        return Ok(result);
    }

    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<string>> Approve(int id)
    {
        var link = await mediator.Send(new ApproveRequestCommand(id));
        return Ok(link);
    }

    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectDto body)
    {
        await mediator.Send(new RejectRequestCommand(id, body.Reason));
        return NoContent();
    }



    [AllowAnonymous]
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ConfirmActivationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConfirmActivationResult>> Confirm([FromBody] ConfirmDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new ConfirmActivationCommand(dto.Token), ct);
        return Ok(result);
    }
}
