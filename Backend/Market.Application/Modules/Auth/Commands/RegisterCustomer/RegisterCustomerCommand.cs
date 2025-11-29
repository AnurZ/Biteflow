using MediatR;

namespace Market.Application.Modules.Auth.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommand : IRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
