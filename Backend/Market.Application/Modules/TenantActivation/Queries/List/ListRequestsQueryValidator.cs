namespace Market.Application.Modules.TenantActivation.Queries.List
{
    public sealed class ListRequestsQueryValidator : AbstractValidator<ListRequestsQuery>
    {
        public ListRequestsQueryValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue);
        }
    }
}
