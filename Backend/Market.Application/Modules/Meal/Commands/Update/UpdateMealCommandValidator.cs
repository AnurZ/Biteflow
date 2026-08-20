using FluentValidation;

namespace Market.Application.Modules.Meal.Commands.Update
{
    public sealed class UpdateMealCommandValidator : AbstractValidator<UpdateMealCommand>
    {
        public UpdateMealCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500);

            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.ImageField)
                .MaximumLength(500);

            RuleFor(x => x.Ingredients)
                .NotNull();

            RuleForEach(x => x.Ingredients)
                .SetValidator(new UpdateMealIngredientDtoValidator());
        }
    }

    public sealed class UpdateMealIngredientDtoValidator
        : AbstractValidator<UpdateMealIngredientDto>
    {
        public UpdateMealIngredientDtoValidator()
        {
            RuleFor(x => x.InventoryItemId)
                .GreaterThan(0);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);

            RuleFor(x => x.UnitType)
                .IsInEnum();
        }
    }
}