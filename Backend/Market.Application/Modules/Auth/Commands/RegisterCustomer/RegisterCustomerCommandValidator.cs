using FluentValidation;

namespace Market.Application.Modules.Auth.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.DisplayName)
            .MaximumLength(200);

        RuleFor(x => x.CaptchaToken)
            .NotEmpty();
    }
}
