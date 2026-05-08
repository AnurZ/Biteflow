using Market.Application.Modules.MealCategory.Commands.CreateMealCategoryCommand;
using Market.Application.Modules.MealCategory.Commands.DeleteMealCategoryCommand;
using Market.Application.Modules.MealCategory.Commands.UpdateMealCategoryCommand;
using Market.Application.Modules.MealCategory.Queries.GetByIdMealCategory;
using Market.Application.Modules.MealCategory.Queries.GetMealCategories;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PolicyNames.StaffMember)]
    public class MealCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MealCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/MealCategory
        [HttpGet]
        public async Task<ActionResult<List<GetMealCategoriesDto>>> GetAll()
        {
            var query = new GetMealCategoryQuery();
            var categories = await _mediator.Send(query);
            return Ok(categories);
        }

        // POST: api/MealCategory
        [HttpPost]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<ActionResult<int>> Create([FromBody] CreateMealCategoryCommand command)
        {
            var categoryId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id = categoryId }, categoryId);
        }

        // GET: api/MealCategory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetMealCategoryByIdDto>> GetById(int id)
        {
            var query = new GetByIdMealCategoryQuery { Id = id };
            var category = await _mediator.Send(query);

            if (category == null)
                return NotFound($"Meal category with ID {id} not found.");

            return Ok(category);
        }

        // DELETE: api/MealCategory/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteMealCategoryCommandDto { Id = id };
            await _mediator.Send(command);
            return NoContent(); // 204 if successfully deleted
        }

        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMealCategoryCommandDto command)
        {
            command.Id = id; // enforce URL ID
            await _mediator.Send(command);
            return Ok(); // just return 200 OK if all went well
        }

    }
}
