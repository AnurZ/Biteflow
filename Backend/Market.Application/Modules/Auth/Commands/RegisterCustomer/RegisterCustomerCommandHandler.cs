using FluentValidation;
using Market.Application.Abstractions;
using Market.Domain.Entities.Identity;
using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Market.Application.Modules.Auth.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICaptchaVerifier _captchaVerifier;
    private readonly ILogger<RegisterCustomerCommandHandler> _logger;

    public RegisterCustomerCommandHandler(
        IAppDbContext db,
        IPasswordHasher<AppUser> hasher,
        UserManager<ApplicationUser> userManager,
        ICaptchaVerifier captchaVerifier,
        ILogger<RegisterCustomerCommandHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _userManager = userManager;
        _captchaVerifier = captchaVerifier;
        _logger = logger;
    }

    public async Task Handle(RegisterCustomerCommand request, CancellationToken ct)
    {
        var captchaOk = await _captchaVerifier.VerifyAsync(request.CaptchaToken, ct);
        if (!captchaOk)
        {
            throw new ValidationException("Captcha validation failed.");
        }

        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? email
            : request.DisplayName.Trim();

        var appUserExists = await _db.Users.AnyAsync(
            u => u.Email.ToLower() == normalizedEmail,
            ct);

        if (appUserExists)
            throw new ValidationException("Email already in use.");

        var identityUserExists = await _userManager.FindByEmailAsync(email);
        if (identityUserExists != null)
            throw new ValidationException("Email already in use.");

        var appUser = new AppUser
        {
            TenantId = SeedConstants.DefaultTenantId,
            RestaurantId = Guid.Empty,
            Email = email,
            DisplayName = displayName,
            IsEmailConfirmed = true,
            IsLocked = false,
            IsEnabled = true,
            TokenVersion = 0
        };

        appUser.PasswordHash = _hasher.HashPassword(appUser, request.Password);

        _db.Users.Add(appUser);
        await _db.SaveChangesAsync(ct);

        var identityUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            TenantId = SeedConstants.DefaultTenantId,
            RestaurantId = null,
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
