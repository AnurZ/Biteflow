using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealIngredient;
using Market.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Meal.Commands.Create
{
    public sealed class CreateMealCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        : IRequestHandler<CreateMealCommand, int>
    {
        public async Task<int> Handle(CreateMealCommand request, CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Meal name is required.");

            var normalizedName = request.Name.Trim().ToLower();

            // 1. Name uniqueness check (per restaurant)
            var nameExists = await db.Meals
                .WhereCurrentRestaurant(tenantContext)
                .AnyAsync(m =>
                    m.Name.ToLower() == normalizedName,
                    cancellationToken);

            if (nameExists)
                throw new ValidationException($"A meal with the name '{request.Name.Trim()}' already exists.");

            if (request.BasePrice < 0)
                throw new ValidationException("BasePrice cannot be negative.");

            // 2. Category validation (tenant-safe)
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await db.MealCategories
                    .WhereCurrentRestaurant(tenantContext)
                    .AnyAsync(c =>
                        c.Id == request.CategoryId.Value,
                        cancellationToken);

                if (!categoryExists)
                    throw new ValidationException($"MealCategoryId {request.CategoryId.Value} is invalid.");
            }

            // 3. Ingredient validation (bulk, no N+1)
            var ingredientIds = request.Ingredients
                .Select(i => i.InventoryItemId)
                .ToList();

            var validIds = (await db.InventoryItems
                .WhereCurrentRestaurant(tenantContext)
                .Where(i => ingredientIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            if (!ingredientIds.All(validIds.Contains))
                throw new ValidationException("One or more InventoryItemIds are invalid.");

            if (request.Ingredients.Any(i => i.Quantity <= 0))
                throw new ValidationException("Ingredient quantity must be greater than zero.");

            // 4. Create meal
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
                RestaurantId = restaurantId,
            };

            // 5. Create ingredients as part of the same EF graph so one save is atomic.
            foreach (var ingredientDto in request.Ingredients)
            {
                newMeal.Ingredients.Add(new MealIngredient
                {
                    InventoryItemId = ingredientDto.InventoryItemId,
                    Quantity = ingredientDto.Quantity,
                    UnitTypes = ingredientDto.UnitType
                });
            }

            db.Meals.Add(newMeal);
            await db.SaveChangesAsync(cancellationToken);

            return newMeal.Id;
        }
    }
}
