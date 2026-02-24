namespace Market.Application.Modules.TenantActivation.Commands.ApproveRequest
{
    public sealed class ApproveRequestCommandValidator : AbstractValidator<ApproveRequestCommand>
    {
        public ApproveRequestCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
