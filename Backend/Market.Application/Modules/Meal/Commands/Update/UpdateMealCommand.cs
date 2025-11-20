using Market.Domain.Common.Enums;
using MediatR;
using System.Collections.Generic;

namespace Market.Application.Modules.Meal.Commands.Update
{
    public sealed class UpdateMealCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public double BasePrice { get; init; }
        public bool IsAvailable { get; init; }
        public bool IsFeatured { get; init; }
        public string ImageField { get; init; } = string.Empty;
        public bool StockManaged { get; init; }
        public int? CategoryId { get; set; }


        public List<UpdateMealIngredientDto> Ingredients { get; init; } = new();
    }

    public sealed class UpdateMealIngredientDto
    {
        public int InventoryItemId { get; init; }
        public double Quantity { get; init; }
        public UnitTypes UnitType { get; init; }
    }
}
