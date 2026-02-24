namespace Market.Application.Modules.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
    {
        public GetOrdersQueryValidator()
        {
            RuleForEach(x => x.Statuses!)
                .IsInEnum()
                .When(x => x.Statuses is { Count: > 0 });
        }
    }
}
