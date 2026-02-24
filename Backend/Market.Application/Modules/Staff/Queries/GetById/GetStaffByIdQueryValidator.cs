namespace Market.Application.Modules.Staff.Queries.GetById;

public sealed class GetStaffByIdQueryValidator : AbstractValidator<GetStaffByIdQuery>
{
    public GetStaffByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
