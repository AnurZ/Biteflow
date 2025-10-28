using Market.Domain.Entities.MealIngredient;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Market.Application.Modules.Meal.Commands.Create;

namespace Market.Application.Modules.Meal.Queries.GetMealIngredients
{
    public sealed class GetMealIngredientsQueryHandler(IAppDbContext db) : IRequestHandler<GetMealIngredientsQuery, List<MealIngredientQueryDto>>
    {
        public async Task<List<MealIngredientQueryDto>> Handle(GetMealIngredientsQuery request, CancellationToken cancellationToken)
        {
            var ingredients = await db.MealIngredients
                .Where(mi => mi.MealId == request.MealId)
                .Include(mi => mi.InventoryItem) // to get InventoryItem name
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
