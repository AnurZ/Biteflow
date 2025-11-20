using Market.Domain.Entities.MealIngredient;
using Market.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Market.Application.Modules.Meal.Commands.Update
{
    public sealed class UpdateMealCommandHandler(IAppDbContext db)
        : IRequestHandler<UpdateMealCommand>
    {
        public async Task Handle(UpdateMealCommand request, CancellationToken cancellationToken)
        {
            var meal = await db.Meals
                .Include(m => m.Ingredients)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (meal is null)
                throw new KeyNotFoundException($"Meal with ID {request.Id} not found.");

           var nameExists = await db.Meals
             .AnyAsync(m => m.Id != request.Id && m.Name == meal.Name, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A meal with the name '{request.Name.Trim()}' already exists.");

            if (request.BasePrice < 0)
                throw new ValidationException("BasePrice cannot be negative.");

            // Update meal basic properties
            meal.Name = request.Name.Trim();
            meal.Description = request.Description?.Trim() ?? string.Empty;
            meal.BasePrice = request.BasePrice;
            meal.IsAvailable = request.IsAvailable;
            meal.IsFeatured = request.IsFeatured;
            meal.ImageField = request.ImageField?.Trim() ?? string.Empty;
            meal.StockManaged = request.StockManaged;
            meal.CategoryId = request.CategoryId;

            // Remove all existing ingredients
            db.MealIngredients.RemoveRange(meal.Ingredients);

            // Check for duplicates in request itself
            var duplicateIds = request.Ingredients
                .GroupBy(i => i.InventoryItemId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Any())
                throw new ValidationException($"Duplicate ingredients in request: {string.Join(", ", duplicateIds)}");

            // Add new ingredients
            foreach (var ingredientDto in request.Ingredients)
            {
                // Validate InventoryItem exists
                var exists = await db.InventoryItems
                    .AnyAsync(i => i.Id == ingredientDto.InventoryItemId, cancellationToken);

                if (!exists)
                    throw new ValidationException($"InventoryItemId {ingredientDto.InventoryItemId} is invalid.");

                if (ingredientDto.Quantity <= 0)
                    throw new ValidationException($"Quantity for InventoryItemId {ingredientDto.InventoryItemId} must be greater than zero.");

                db.MealIngredients.Add(new MealIngredient
                {
                    MealId = meal.Id,
                    InventoryItemId = ingredientDto.InventoryItemId,
                    Quantity = ingredientDto.Quantity,
                    UnitTypes = ingredientDto.UnitType
                });
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
