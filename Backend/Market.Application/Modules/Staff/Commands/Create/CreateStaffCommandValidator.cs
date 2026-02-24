using Market.Shared.Constants;

namespace Market.Application.Modules.Staff.Commands.Create;

public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.Staff,
        RoleNames.Waiter,
        RoleNames.Kitchen
    };

    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.AppUserId).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(200);

        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required.")
            .MaximumLength(50);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.HourlyRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HourlyRate.HasValue);

        RuleFor(x => x.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || AllowedRoles.Contains(role.Trim()))
            .WithMessage("Role is invalid.");

        RuleFor(x => x.PlainPassword)
            .MinimumLength(8)
            .When(x => !string.IsNullOrWhiteSpace(x.PlainPassword));

        RuleFor(x => x)
            .Must(x => !x.ShiftStart.HasValue || !x.ShiftEnd.HasValue || x.ShiftEnd.Value > x.ShiftStart.Value)
            .WithMessage("ShiftEnd must be after ShiftStart.");
    }
}
