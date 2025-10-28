using MediatR;
using System.Collections.Generic;

namespace Market.Application.Modules.Meal.Queries.GetMealIngredients
{
    public sealed class GetMealIngredientsQuery : IRequest<List<MealIngredientQueryDto>>
    {
        public int MealId { get; set; }
    }

    public sealed class MealIngredientQueryDto
    {
        public int InventoryItemId { get; set; }
        public string InventoryItemName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string UnitType { get; set; } = string.Empty;
    }
}
