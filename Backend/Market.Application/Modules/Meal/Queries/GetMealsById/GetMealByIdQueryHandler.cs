using Market.Application.Modules.Meal.Queries.GetMealIngredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Queries.GetMealsById
{
    public sealed class GetMealByIdQueryHandler(IAppDbContext db)
        : IRequestHandler<GetMealByIdQuery, GetMealByIdDto>
    {
        public async Task<GetMealByIdDto> Handle(GetMealByIdQuery request, CancellationToken ct)
        {
            var meal = await db.Meals
                .Include(m => m.Ingredients)
                    .ThenInclude(mi => mi.InventoryItem)
                .FirstOrDefaultAsync(m => m.Id == request.Id, ct);

            if (meal == null)
                throw new KeyNotFoundException($"Meal with ID {request.Id} not found.");

            return new GetMealByIdDto
            {
                Id = meal.Id,
                Name = meal.Name,
                Description = meal.Description,
                BasePrice = meal.BasePrice,
                IsAvailable = meal.IsAvailable,
                IsFeatured = meal.IsFeatured,
                ImageField = meal.ImageField,
                Ingredients = meal.Ingredients.Select(mi => new MealIngredientQueryDto
                {
                    InventoryItemId = mi.InventoryItemId,
                    InventoryItemName = mi.InventoryItem.Name,
                    Quantity = mi.Quantity,
                    UnitType = mi.UnitTypes.ToString()
                }).ToList()
            };
        }
    }
}
