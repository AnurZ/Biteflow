using Market.Application.Modules.InventoryItem.Commands.Create;
using Market.Application.Modules.InventoryItem.Commands.Delete;
using Market.Application.Modules.InventoryItem.Commands.Update;
using Market.Application.Modules.InventoryItem.Queries.List;
using Market.Application.Modules.InventoryItem.Querries.GetById;
using Market.Application.Modules.InventoryItem.Querries.List;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class InventoryItemController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<PageResult<ListInventoryItemsDto>> List([FromQuery] ListInventoryItemsQuery q, CancellationToken ct)
        => await sender.Send(q, ct);

    [HttpGet("{id:int}")]
    public async Task<GetInventoryItemByIdDto> GetById(int id, CancellationToken ct)
        => await sender.Send(new GetInventoryItemByIdQuery { Id = id }, ct);

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateInventoryItemCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateInventoryItemCommand cmd, CancellationToken ct)
    {
        cmd.Id = id;
        await sender.Send(cmd, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteInventoryItemCommand { Id = id }, ct);
        return NoContent();
    }
}
