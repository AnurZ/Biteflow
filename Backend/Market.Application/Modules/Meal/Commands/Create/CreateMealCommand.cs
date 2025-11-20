using Market.Domain.Common.Enums;
using MediatR;

namespace Market.Application.Modules.Meal.Commands.Create
{
    public sealed class CreateMealCommand : IRequest<int>
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public double BasePrice { get; init; }
        public bool IsAvailable { get; init; }
        public bool IsFeatured { get; init; }
        public string ImageField { get; init; } = string.Empty;
        public bool StockManaged { get; init; }
        public int? CategoryId { get; set; }

        // Ingredients for this meal
        public List<MealIngredientDto> Ingredients { get; init; } = new();
    }

    public sealed class MealIngredientDto
    {
        public int InventoryItemId { get; init; }
        public double Quantity { get; init; }
        public UnitTypes UnitType { get; init; }
    }
}
