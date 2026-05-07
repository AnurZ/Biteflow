using FluentValidation;
using Market.Application.Abstractions;
using Market.Domain.Entities.IdentityV2;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Market.Application.Modules.Auth.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICaptchaVerifier _captchaVerifier;
    private readonly ILogger<RegisterCustomerCommandHandler> _logger;
    private readonly IPublicTenantResolver _publicTenantResolver;

    public RegisterCustomerCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICaptchaVerifier captchaVerifier,
        ILogger<RegisterCustomerCommandHandler> logger,
        IPublicTenantResolver publicTenantResolver)
    {
        _userManager = userManager;
        _captchaVerifier = captchaVerifier;
        _logger = logger;
        _publicTenantResolver = publicTenantResolver;
    }

    public async Task Handle(RegisterCustomerCommand request, CancellationToken ct)
    {
        var captchaOk = await _captchaVerifier.VerifyAsync(request.CaptchaToken, ct);
        if (!captchaOk)
        {
            throw new ValidationException("Captcha validation failed.");
        }

        var email = request.Email.Trim();
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? email
            : request.DisplayName.Trim();
        var publicTenant = await _publicTenantResolver.ResolveRequiredAsync(ct);

        var identityUserExists = await _userManager.FindByEmailAsync(email);
        if (identityUserExists != null)
            throw new ValidationException("Email already in use.");

        var identityUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            TenantId = publicTenant.TenantId,
            RestaurantId = publicTenant.RestaurantId,
            EmailConfirmed = true,
            IsEnabled = true
        };

        var create = await _userManager.CreateAsync(identityUser, request.Password);
        if (!create.Succeeded)
        {
            var errors = string.Join(", ", create.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create identity user for {Email}: {Errors}", email, errors);
            throw new ValidationException($"Failed to create user: {errors}");
        }

        if (!await _userManager.IsInRoleAsync(identityUser, RoleNames.Customer))
        {
            var roleResult = await _userManager.AddToRoleAsync(identityUser, RoleNames.Customer);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to assign customer role for {Email}: {Errors}", email, errors);
                throw new ValidationException($"Failed to assign role: {errors}");
            }
        }

        _logger.LogInformation("Registered new customer user {Email}", email);
    }
}
