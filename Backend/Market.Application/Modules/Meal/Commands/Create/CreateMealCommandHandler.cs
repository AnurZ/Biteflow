using Market.Application.Modules.Staff.Commands.Create;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealIngredient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Commands.Create
{
    public sealed class CreateMealCommandHandler(IAppDbContext db)
        : IRequestHandler<CreateMealCommand, int>
    {
        public async Task<int> Handle(CreateMealCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Meal name is required.");

            var nameExists = await db.Meals
                .AnyAsync(m => m.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

            if (nameExists)
                throw new ValidationException($"A meal with the name '{request.Name.Trim()}' already exists.");


            if (request.BasePrice < 0)
                throw new ValidationException("BasePrice cannot be negative.");


            var newMeal = new Domain.Entities.Meal.Meal
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                BasePrice = request.BasePrice,
                IsAvailable = request.IsAvailable,
                IsFeatured = request.IsFeatured,
                ImageField = request.ImageField?.Trim() ?? string.Empty,
                StockManaged = request.StockManaged,
                CategoryId = request.CategoryId,

            };

            db.Meals.Add(newMeal);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var ingredientDto in request.Ingredients)
            {
                // Optional: check if InventoryItem exists
                var exists = await db.InventoryItems.AnyAsync(i => i.Id == ingredientDto.InventoryItemId, cancellationToken);
                if (!exists)
                    throw new ValidationException($"InventoryItemId {ingredientDto.InventoryItemId} is invalid.");

                if (ingredientDto.Quantity <= 0)
                    throw new ValidationException($"Quantity for InventoryItemId {ingredientDto.InventoryItemId} must be greater than zero.");


                var mealIngredient = new MealIngredient
                {
                    MealId = newMeal.Id,
                    InventoryItemId = ingredientDto.InventoryItemId,
                    Quantity = ingredientDto.Quantity,
                    UnitTypes = ingredientDto.UnitType
                };

                db.MealIngredients.Add(mealIngredient);
            }


            await db.SaveChangesAsync(cancellationToken);

            return newMeal.Id;
        }
    }
}
