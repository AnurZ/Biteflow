using MediatR;
using System.Collections.Generic;

using MediatR;

namespace Market.Application.Modules.Meal.Queries.GetList
{
    public sealed class GetMealsQuery : BasePagedQuery<MealDto>
    {
        public string? Search { get; init; }
        public string? Sort { get; init; }
        public int? CategoryId { get; init; }
    }

    public sealed class MealDto
    {
        public int Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double BasePrice { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public bool StockManaged { get; set; }
        public string ImageField { get; set; } = string.Empty;
        public int? CategoryId { get; set; }

        public int IngredientsCount { get; set; }
    }
}
