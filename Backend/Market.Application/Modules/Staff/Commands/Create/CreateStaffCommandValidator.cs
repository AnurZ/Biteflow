using Market.Shared.Constants;
using Microsoft.Extensions.Options;

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

    public CreateStaffCommandValidator(IOptions<IdentityOptions> identityOptions)
    {
        var password = identityOptions.Value.Password;

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
            .MinimumLength(password.RequiredLength)
                .WithMessage($"Password must be at least {password.RequiredLength} characters long.")
            .Must(value => !password.RequireDigit || HasDigit(value))
                .WithMessage("Password must contain at least one digit.")
            .Must(value => !password.RequireLowercase || HasLowercase(value))
                .WithMessage("Password must contain at least one lowercase letter.")
            .Must(value => !password.RequireUppercase || HasUppercase(value))
                .WithMessage("Password must contain at least one uppercase letter.")
            .Must(value => !password.RequireNonAlphanumeric || HasNonAlphanumeric(value))
                .WithMessage("Password must contain at least one non-alphanumeric character.")
            .When(x => !string.IsNullOrWhiteSpace(x.PlainPassword));

        RuleFor(x => x)
            .Must(x => !x.ShiftStart.HasValue || !x.ShiftEnd.HasValue || x.ShiftEnd.Value > x.ShiftStart.Value)
            .WithMessage("ShiftEnd must be after ShiftStart.");
    }

    private static bool HasDigit(string? value)
        => value?.Any(char.IsDigit) == true;

    private static bool HasLowercase(string? value)
        => value?.Any(char.IsLower) == true;

    private static bool HasUppercase(string? value)
        => value?.Any(char.IsUpper) == true;

    private static bool HasNonAlphanumeric(string? value)
        => value?.Any(c => !char.IsLetterOrDigit(c)) == true;
}
