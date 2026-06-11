namespace Market.Application.Modules.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
    {
        public GetOrdersQueryValidator()
        {
            RuleFor(x => x.Paging.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0.");

            RuleForEach(x => x.Statuses!)
                .IsInEnum()
                .When(x => x.Statuses is { Count: > 0 });
        }
    }
}
