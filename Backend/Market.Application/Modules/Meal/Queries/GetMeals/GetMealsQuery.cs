using MediatR;
using System.Collections.Generic;

namespace Market.Application.Modules.Meal.Queries.GetList
{
    public sealed class GetMealsQuery : IRequest<List<MealDto>>
    {
    }

    public sealed class MealDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double BasePrice { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public string ImageField { get; set; } = string.Empty;

        // Optional: include ingredients count
        public int IngredientsCount { get; set; }
    }
}
