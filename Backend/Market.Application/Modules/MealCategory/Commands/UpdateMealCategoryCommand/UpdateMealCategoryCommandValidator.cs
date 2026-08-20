using FluentValidation;

namespace Market.Application.Modules.MealCategory.Commands.UpdateMealCategoryCommand
{
    public sealed class UpdateMealCategoryCommandDtoValidator
        : AbstractValidator<UpdateMealCategoryCommandDto>
    {
        public UpdateMealCategoryCommandDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}