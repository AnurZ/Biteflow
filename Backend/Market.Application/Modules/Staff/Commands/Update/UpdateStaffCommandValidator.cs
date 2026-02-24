namespace Market.Application.Modules.Staff.Commands.Update;

public sealed class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required.")
            .MaximumLength(50);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100);

        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Salary.HasValue);

        RuleFor(x => x.HourlyRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HourlyRate.HasValue);

        RuleFor(x => x)
            .Must(x => !x.HireDate.HasValue || !x.TerminationDate.HasValue || x.TerminationDate.Value >= x.HireDate.Value)
            .WithMessage("TerminationDate must be greater than or equal to HireDate.");

        RuleFor(x => x)
            .Must(x => !x.ShiftStart.HasValue || !x.ShiftEnd.HasValue || x.ShiftEnd.Value > x.ShiftStart.Value)
            .WithMessage("ShiftEnd must be after ShiftStart.");
    }
}
