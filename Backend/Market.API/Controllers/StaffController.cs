// Market.API/Controllers/StaffController.cs
using Market.Application.Modules.Staff.Commands.Create;
using Market.Application.Modules.Staff.Commands.Delete;
using Market.Application.Modules.Staff.Commands.Update;
using Market.Application.Modules.Staff.Queries.GetById;
using Market.Application.Modules.Staff.Queries.List;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StaffController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PolicyNames.StaffMember)]
    public async Task<PageResult<ListStaffItemDto>> List([FromQuery] ListStaffQuery q, CancellationToken ct)
        => await sender.Send(q, ct);

    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyNames.StaffMember)]
    public async Task<GetStaffByIdDto> GetById(int id, CancellationToken ct)
        => await sender.Send(new GetStaffByIdQuery { Id = id }, ct);

    [HttpPost]
    [Authorize(Policy = PolicyNames.RestaurantAdmin)]
    public async Task<ActionResult<int>> Create(CreateStaffCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.RestaurantAdmin)]
    public async Task<IActionResult> Update(int id, UpdateStaffCommand cmd, CancellationToken ct)
    {
        cmd.Id = id;
        await sender.Send(cmd, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.RestaurantAdmin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteStaffCommand { Id = id }, ct);
        return NoContent();
    }
}
