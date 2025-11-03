using Market.Application.Modules.InventoryItem.Queries.GetByName;
using Market.Application.Modules.Meal.Commands.Create;
using Market.Application.Modules.Meal.Commands.Delete;
using Market.Application.Modules.Meal.Commands.Update;

//using Market.Application.Modules.Meal.Commands.Delete;
//using Market.Application.Modules.Meal.Commands.Update;
//using Market.Application.Modules.Meal.Queries.GetById;
using Market.Application.Modules.Meal.Queries.GetList;
using Market.Application.Modules.Meal.Queries.GetMealIngredients;
using Market.Application.Modules.Meal.Queries.GetMealsByName;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MealController(ISender sender) : ControllerBase
{
    // GET: api/meal
    [HttpGet]
    public async Task<List<MealDto>> GetList(CancellationToken ct)
        => await sender.Send(new GetMealsQuery(), ct);

    // GET: api/meal/{id}
    [HttpGet("{id:int}")]
    public async Task<GetMealByIdDto> GetById(int id, CancellationToken ct)
        => await sender.Send(new GetMealByIdQuery { Id = id }, ct);

    [HttpGet("by-name")]
    public async Task<PageResult<GetMealsByNameDto>> GetByName([FromQuery] GetMealsByNameQuery q, CancellationToken ct)
        => await sender.Send(q, ct);

    // GET: api/meal/{id}/ingredients
    [HttpGet("{id:int}/ingredients")]
    public async Task<List<MealIngredientQueryDto>> GetIngredients(int id, CancellationToken ct)
        => await sender.Send(new GetMealIngredientsQuery { MealId = id }, ct);

    // POST: api/meal
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateMealCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    //// PUT: api/meal/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateMealCommand cmd, CancellationToken ct)
    {
        cmd.Id = id;
        await sender.Send(cmd, ct);
        return NoContent();
    }

    // DELETE: api/meal/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteMealCommand { Id = id }, ct);
        return NoContent();
    }
}
