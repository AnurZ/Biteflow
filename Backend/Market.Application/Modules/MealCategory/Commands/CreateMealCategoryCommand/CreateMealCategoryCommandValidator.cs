using FluentValidation;

namespace Market.Application.Modules.MealCategory.Commands.CreateMealCategoryCommand
{
    public sealed class CreateMealCategoryCommandValidator
        : AbstractValidator<CreateMealCategoryCommand>
    {
        public CreateMealCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}