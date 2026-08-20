using FluentValidation;

namespace Market.Application.Modules.Meal.Commands.Create;

public sealed class CreateMealCommandValidator : AbstractValidator<CreateMealCommand>
{
    public CreateMealCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Ingredients)
            .NotNull();

        RuleForEach(x => x.Ingredients)
            .ChildRules(ingredient =>
            {
                ingredient.RuleFor(x => x.Quantity)
                    .GreaterThan(0);
            });
    }
}