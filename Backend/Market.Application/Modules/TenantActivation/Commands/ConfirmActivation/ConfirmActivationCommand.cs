namespace Market.Application.Modules.TenantActivation.Commands.ConfirmActivation;

public sealed record ConfirmActivationCommand(string token) : IRequest<ConfirmActivationResult>;
