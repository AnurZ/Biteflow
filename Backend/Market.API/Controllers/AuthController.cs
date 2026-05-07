using Market.Application.Modules.Auth.Commands.RegisterCustomer;
using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator, UserManager<ApplicationUser> userManager) : ControllerBase
{
    public sealed record SetPasswordDto(Guid UserId, string Token, string Password);

    [HttpPost("register/customer")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok();
    }

    [HttpPost("set-password")]
    [AllowAnonymous]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
    {
        if (dto.UserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.Token) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("User id, token, and password are required.");
        }

        var user = await userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null)
        {
            return BadRequest("Password setup link is invalid or expired.");
        }

        var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
        if (result.Succeeded)
        {
            return Ok();
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        return BadRequest(string.IsNullOrWhiteSpace(errors)
            ? "Password setup link is invalid or expired."
            : errors);
    }
}
