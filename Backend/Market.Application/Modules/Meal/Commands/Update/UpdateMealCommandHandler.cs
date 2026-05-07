using Market.Domain.Entities.MealIngredient;
using Market.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Meal.Commands.Update
{
    public sealed class UpdateMealCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        : IRequestHandler<UpdateMealCommand>
    {
        public async Task Handle(UpdateMealCommand request, CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RestaurantId;

            if (restaurantId == null || restaurantId == Guid.Empty)
                throw new ValidationException("Restaurant context is missing.");

            var meal = await db.Meals
                .WhereNullableRestaurantOwned(tenantContext)
                .Include(m => m.Ingredients)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (meal is null)
                throw new KeyNotFoundException($"Meal with ID {request.Id} not found.");

            var normalizedName = request.Name.Trim().ToLower();

            // 1. NAME UNIQUENESS (FIXED + SCOPE SAFE)
            var nameExists = await db.Meals
                .WhereNullableRestaurantOwned(tenantContext)
                .AnyAsync(m =>
                    m.Id != request.Id &&
                    m.RestaurantId == restaurantId &&
                    m.Name.ToLower() == normalizedName,
                    cancellationToken);

            if (nameExists)
                throw new ValidationException($"A meal with the name '{request.Name.Trim()}' already exists.");

            if (request.BasePrice < 0)
                throw new ValidationException("BasePrice cannot be negative.");

            // 2. CATEGORY VALIDATION (FIXED CROSS-TENANT)
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await db.MealCategories
                    .WhereNullableRestaurantOwned(tenantContext)
                    .AnyAsync(c =>
                        c.Id == request.CategoryId.Value &&
                        c.RestaurantId == restaurantId,
                        cancellationToken);

                if (!categoryExists)
                    throw new ValidationException($"MealCategoryId {request.CategoryId.Value} is invalid.");
            }

            // 3. INGREDIENT VALIDATION (BULK - NO N+1)
            var ingredientIds = request.Ingredients
                .Select(i => i.InventoryItemId)
                .ToList();

            var validIds = (await db.InventoryItems
                .WhereNullableRestaurantOwned(tenantContext)
                .Where(i => ingredientIds.Contains(i.Id) && i.RestaurantId == restaurantId)
                .Select(i => i.Id)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            if (!ingredientIds.All(validIds.Contains))
                throw new ValidationException("One or more InventoryItemIds are invalid.");

            if (request.Ingredients.Any(i => i.Quantity <= 0))
                throw new ValidationException("Quantity must be greater than zero.");

            // 4. DUPLICATES IN REQUEST
            var duplicateIds = request.Ingredients
                .GroupBy(i => i.InventoryItemId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Any())
                throw new ValidationException($"Duplicate ingredients: {string.Join(", ", duplicateIds)}");

            // 5. UPDATE MEAL
            meal.Name = request.Name.Trim();
            meal.Description = request.Description?.Trim() ?? string.Empty;
            meal.BasePrice = request.BasePrice;
            meal.IsAvailable = request.IsAvailable;
            meal.IsFeatured = request.IsFeatured;
            meal.ImageField = request.ImageField?.Trim() ?? string.Empty;
            meal.StockManaged = request.StockManaged;
            meal.CategoryId = request.CategoryId;

            // 6. INGREDIENTS REPLACE
            db.MealIngredients.RemoveRange(meal.Ingredients);

            foreach (var ingredient in request.Ingredients)
            {
                db.MealIngredients.Add(new MealIngredient
                {
                    MealId = meal.Id,
                    InventoryItemId = ingredient.InventoryItemId,
                    Quantity = ingredient.Quantity,
                    UnitTypes = ingredient.UnitType
                });
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}