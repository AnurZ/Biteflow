using MediatR;
using Microsoft.EntityFrameworkCore;
using Market.Application.Modules.Meal.Commands.Create;

namespace Market.Application.Modules.Meal.Queries.GetMealIngredients
{
    public sealed class GetMealIngredientsQueryHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
        : IRequestHandler<GetMealIngredientsQuery, List<MealIngredientQueryDto>>
    {
        public async Task<List<MealIngredientQueryDto>> Handle(
            GetMealIngredientsQuery request,
            CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var ingredients = await db.MealIngredients
                .AsNoTracking()
                .Where(mi =>
                    mi.MealId == request.MealId &&
                    mi.Meal.RestaurantId == restaurantId)
                .Include(mi => mi.InventoryItem)
                .Select(mi => new MealIngredientQueryDto
                {
                    InventoryItemId = mi.InventoryItemId,
                    InventoryItemName = mi.InventoryItem.Name,
                    Quantity = mi.Quantity,
                    UnitType = mi.UnitTypes.ToString()
                })
                .ToListAsync(cancellationToken);

            return ingredients;
        }
    }
}