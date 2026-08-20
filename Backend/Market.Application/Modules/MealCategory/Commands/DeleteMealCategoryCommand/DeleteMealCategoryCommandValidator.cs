using FluentValidation;

namespace Market.Application.Modules.MealCategory.Commands.DeleteMealCategoryCommand
{
    public sealed class DeleteMealCategoryCommandDtoValidator
        : AbstractValidator<DeleteMealCategoryCommandDto>
    {
        public DeleteMealCategoryCommandDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);
        }
    }
}