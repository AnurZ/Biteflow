namespace Market.Application.Modules.Staff.Commands.Delete;

public sealed class DeleteStaffCommandValidator : AbstractValidator<DeleteStaffCommand>
{
    public DeleteStaffCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
