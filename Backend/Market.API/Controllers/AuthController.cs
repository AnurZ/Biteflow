using Market.Application.Modules.Auth.Commands.RegisterCustomer;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register/customer")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok();
    }
}
