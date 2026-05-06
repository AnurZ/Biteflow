namespace Market.Application.Modules.TenantActivation.Commands.ConfirmActivation;

public sealed record ConfirmActivationResult(
    Guid TenantId,
    string AdminUsername);
