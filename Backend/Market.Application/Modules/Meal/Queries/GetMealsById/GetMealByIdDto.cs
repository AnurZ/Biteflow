using Market.Application.Modules.Meal.Queries.GetMealIngredients;

public sealed class GetMealByIdDto
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
