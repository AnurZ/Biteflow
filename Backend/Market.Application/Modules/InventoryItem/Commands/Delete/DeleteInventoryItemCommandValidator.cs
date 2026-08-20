using FluentValidation;

namespace Market.Application.Modules.InventoryItem.Commands.Delete
{
    public sealed class DeleteInventoryItemCommandValidator
        : AbstractValidator<DeleteInventoryItemCommand>
    {
        public DeleteInventoryItemCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);
        }
    }
}