namespace Market.Application.Modules.TenantActivation.Commands.RejectRequest
{
    public sealed class RejectRequestCommandValidator : AbstractValidator<RejectRequestCommand>
    {
        public RejectRequestCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Reason).NotEmpty();
        }
    }
}
