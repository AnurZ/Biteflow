namespace Market.Application.Modules.TenantActivation.Commands.Submit
{
    public sealed class SubmitDraftCommandValidator : AbstractValidator<SubmitDraftCommand>
    {
        public SubmitDraftCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
