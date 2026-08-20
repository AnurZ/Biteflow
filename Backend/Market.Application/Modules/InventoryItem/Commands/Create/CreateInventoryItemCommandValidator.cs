using FluentValidation;

namespace Market.Application.Modules.InventoryItem.Commands.Create
{
    public sealed class CreateInventoryItemCommandValidator
        : AbstractValidator<CreateInventoryItemCommand>
    {
        public CreateInventoryItemCommandValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Sku)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.UnitType)
                .IsInEnum();

            RuleFor(x => x.ReorderQty)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.ReorderFrequency)
                .GreaterThan(0);

            RuleFor(x => x.CurrentQty)
                .GreaterThanOrEqualTo(0);
        }
    }
}