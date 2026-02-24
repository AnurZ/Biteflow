namespace Market.Application.Modules.TenantActivation.Commands.ConfirmActivation;

public sealed class ConfirmActivationCommandValidator : AbstractValidator<ConfirmActivationCommand>
{
    public ConfirmActivationCommandValidator()
    {
        RuleFor(x => x.token).NotEmpty();
    }
}
