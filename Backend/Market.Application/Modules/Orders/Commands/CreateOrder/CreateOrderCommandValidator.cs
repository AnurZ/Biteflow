namespace Market.Application.Modules.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.DiningTableId)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("DiningTableId is required.")
                .GreaterThan(0)
                .WithMessage("DiningTableId is required.");

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
            RuleFor(x => x.Quantity)
                .GreaterThan(0);

            When(x => x.IsCustom, () =>
            {
                RuleFor(x => x.MealId)
                    .Null()
                    .WithMessage("Custom order items cannot reference a meal.");

                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(256);

                RuleFor(x => x.UnitPrice)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .WithMessage("Custom order item unit price is required.")
                    .GreaterThan(0);
            });

            When(x => !x.IsCustom, () =>
            {
                RuleFor(x => x.MealId)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .WithMessage("MealId is required for menu order items.")
                    .GreaterThan(0)
                    .WithMessage("MealId is required for menu order items.");
            });
        }
    }
}
