using FluentValidation;
using Microsoft.Extensions.Options;

namespace Market.Application.Modules.Auth.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator(IOptions<IdentityOptions> identityOptions)
    {
        var password = identityOptions.Value.Password;

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(password.RequiredLength)
                .WithMessage($"Password must be at least {password.RequiredLength} characters long.")
            .Must(value => !password.RequireDigit || HasDigit(value))
                .WithMessage("Password must contain at least one digit.")
            .Must(value => !password.RequireLowercase || HasLowercase(value))
                .WithMessage("Password must contain at least one lowercase letter.")
            .Must(value => !password.RequireUppercase || HasUppercase(value))
                .WithMessage("Password must contain at least one uppercase letter.")
            .Must(value => !password.RequireNonAlphanumeric || HasNonAlphanumeric(value))
                .WithMessage("Password must contain at least one non-alphanumeric character.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(200);

        RuleFor(x => x.CaptchaToken)
            .NotEmpty();
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
