using Market.Application.Modules.Meal.Queries.GetMealIngredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Queries.GetMealsByName
{
    public sealed class GetMealsByNameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double BasePrice { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public string ImageField { get; set; } = string.Empty;
        public List<MealIngredientQueryDto> Ingredients { get; set; } = new();
    }
}
