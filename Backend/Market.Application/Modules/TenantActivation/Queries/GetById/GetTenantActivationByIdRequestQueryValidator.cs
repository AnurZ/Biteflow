namespace Market.Application.Modules.TenantActivation.Queries.GetById
{
    public sealed class GetTenantActivationByIdRequestQueryValidator : AbstractValidator<GetTenantActivationByIdRequestQuery>
    {
        public GetTenantActivationByIdRequestQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
