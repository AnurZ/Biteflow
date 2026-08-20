using FluentValidation;

namespace Market.Application.Modules.Meal.Commands.Delete
{
    public sealed class DeleteMealCommandValidator : AbstractValidator<DeleteMealCommand>
    {
        public DeleteMealCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);
        }
    }
}