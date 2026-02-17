using Market.Application.Abstractions;
using Market.Application.Common;
using Market.Application.Common.Exceptions;
using Market.Application.Modules.TenantActivation.Commands.ApproveRequest;
using Market.Application.Modules.TenantActivation.Commands.ConfirmActivation;
using Market.Application.Modules.TenantActivation.Commands.Create;
using Market.Application.Modules.TenantActivation.Commands.RejectRequest;
using Market.Application.Modules.TenantActivation.Commands.Submit;
using Market.Application.Modules.TenantActivation.Commands.Update;
using Market.Application.Modules.TenantActivation.Queries.GetById;
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
    // --- DTOs for request bodies ---
    public sealed record RejectDto(string Reason);
    public sealed record ConfirmDto(string Token);

    // ---------- Public (tenant) actions ----------

    // Create draft
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] CreateDraftCommand cmd)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var id = await mediator.Send(cmd);   // expect: returns int id
        return Ok(id);
    }

    // Update draft
    [AllowAnonymous]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDraftCommand body)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (id != body.Id) return BadRequest("Route id and payload id differ.");
        try
        {
            await mediator.Send(body);
            return NoContent();
        }
        catch (MarketNotFoundException)
        {
            return NotFound();
        }
    }

    // Submit draft
    [AllowAnonymous]
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            await mediator.Send(new SubmitDraftCommand(id));
            return NoContent();
        }
        catch (MarketNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            // e.g., not in Draft
            return Conflict(ex.Message);
        }
    }

    // Get one
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ActivationDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivationDraftDto>> Get(int id)
    {
        try
        {
            var dto = await mediator.Send(new GetTenantActivationByIdRequestQuery(id));
            return Ok(dto);
        }
        catch (MarketNotFoundException)
        {
            return NotFound();
        }
    }

    // ---------- Admin actions ----------

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

    // Approve (issues link internally and sets Approved)
    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<string>> Approve(int id)
    {
        try
        {
            // ApproveRequestCommand MUST be int-based: record ApproveRequestCommand(int Id) : IRequest<string>;
            var link = await mediator.Send(new ApproveRequestCommand(id));
            return Ok(link); // FE or mailer will use this URL
        }
        catch (MarketNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            // e.g., Only submitted requests can be approved.
            return Conflict(ex.Message);
        }
    }

    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectDto body)
    {
        try
        {
            await mediator.Send(new RejectRequestCommand(id, body.Reason));
            return NoContent();
        }
        catch (MarketNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            // e.g., cannot reject after activation
            return Conflict(ex.Message);
        }
    }

    // ---------- Activation landing (public) ----------

    [AllowAnonymous]
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ConfirmActivationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConfirmActivationResult>> Confirm([FromBody] ConfirmDto dto, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new ConfirmActivationCommand(dto.Token), ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            // invalid/expired/consumed token
            return Unauthorized("Activation link is invalid or expired.");
        }
        catch (MarketNotFoundException)
        {
            return NotFound("Request not found.");
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("Activation allowed only after approval", StringComparison.OrdinalIgnoreCase)
               || ex.Message.Contains("Only submitted requests can be approved", StringComparison.OrdinalIgnoreCase))
        {
            // domain guard from MarkActivated / Approve
            return Conflict(ex.Message);
        }
    }
}
