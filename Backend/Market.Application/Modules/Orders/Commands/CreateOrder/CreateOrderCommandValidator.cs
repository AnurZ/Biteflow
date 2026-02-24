namespace Market.Application.Modules.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => x.DiningTableId.HasValue || x.TableNumber.HasValue)
                .WithMessage("Either DiningTableId or TableNumber must be provided.");

            RuleFor(x => x.Items)
                .NotNull()
                .NotEmpty()
                .WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.Items)
                .SetValidator(new CreateOrderItemDtoValidator());
        }
    }

    public sealed class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0);
        }
    }
}
