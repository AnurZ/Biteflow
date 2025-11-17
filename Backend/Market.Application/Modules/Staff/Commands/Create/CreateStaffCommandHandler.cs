using Market.Domain.Entities.Identity;
using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Staff;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Market.Application.Modules.Staff.Commands.Create;

public sealed class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, int>
{
    private static readonly string[] AllowedRoles =
    {
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.Staff
    };

    private readonly IAppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateStaffCommandHandler> _logger;

    public CreateStaffCommandHandler(
        IAppDbContext db,
        IPasswordHasher<AppUser> hasher,
        UserManager<ApplicationUser> userManager,
        ILogger<CreateStaffCommandHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<int> Handle(CreateStaffCommand r, CancellationToken ct)
    {
        var tenantId = SeedConstants.DefaultTenantId;

        if (string.IsNullOrWhiteSpace(r.FirstName) || string.IsNullOrWhiteSpace(r.LastName))
            throw new ValidationException("FirstName and LastName are required.");

        var targetRole = NormalizeRole(r.Role);

        var email = r.Email?.Trim();
        if (r.AppUserId == 0 && string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email is required when creating a new user.");

        var displayName = string.IsNullOrWhiteSpace(r.DisplayName)
            ? $"{r.FirstName} {r.LastName}".Trim()
            : r.DisplayName!.Trim();

        var plainPassword = string.IsNullOrWhiteSpace(r.PlainPassword)
            ? Guid.NewGuid().ToString("N")
            : r.PlainPassword!;

        var appUserId = await EnsureLegacyUserAsync(r.AppUserId, email!, displayName, plainPassword, tenantId, ct);
        var identityUser = await EnsureIdentityUserAsync(email!, displayName, plainPassword, tenantId, targetRole, ct);

        var profile = new EmployeeProfile
        {
            TenantId = tenantId,
            AppUserId = appUserId,
            ApplicationUserId = identityUser.Id,
            Position = r.Position.Trim(),
            FirstName = r.FirstName.Trim(),
            LastName = r.LastName.Trim(),
            PhoneNumber = r.PhoneNumber,
            HireDate = r.HireDate,
            HourlyRate = r.HourlyRate,
            EmploymentType = r.EmploymentType,
            ShiftType = r.ShiftType,
            ShiftStart = r.ShiftStart,
            ShiftEnd = r.ShiftEnd,
            IsActive = r.IsActive,
            Notes = r.Notes
        };

        _db.EmployeeProfiles.Add(profile);
        await _db.SaveChangesAsync(ct);

        return profile.Id;
    }

    private async Task<int> EnsureLegacyUserAsync(
        int existingAppUserId,
        string email,
        string displayName,
        string plainPassword,
        Guid tenantId,
        CancellationToken ct)
    {
        if (existingAppUserId > 0)
        {
            var exists = await _db.Users.AnyAsync(u => u.Id == existingAppUserId, ct);
            if (!exists) throw new ValidationException("AppUserId is invalid.");
            return existingAppUserId;
        }

        var normalizedEmail = email.Trim();
        var emailTaken = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (emailTaken) throw new MarketConflictException("Email already in use.");

        var user = new AppUser
        {
            TenantId = tenantId,
            RestaurantId = Guid.Empty,
            Email = normalizedEmail,
            DisplayName = displayName,
            IsEmailConfirmed = false,
            IsLocked = false,
            IsEnabled = true,
            TokenVersion = 0
        };

        user.PasswordHash = _hasher.HashPassword(user, plainPassword);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return user.Id;
    }

    private async Task<ApplicationUser> EnsureIdentityUserAsync(
        string email,
        string displayName,
        string plainPassword,
        Guid tenantId,
        string role,
        CancellationToken ct)
    {
        var normalizedEmail = email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                DisplayName = displayName,
                TenantId = tenantId,
                RestaurantId = Guid.Empty,
                EmailConfirmed = false,
                IsEnabled = true
            };

            var create = await _userManager.CreateAsync(user, plainPassword);
            if (!create.Succeeded)
            {
                var message = string.Join(", ", create.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create identity user {Email}: {Message}", normalizedEmail, message);
                throw new ValidationException($"Failed to create identity user: {message}");
            }
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var message = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new ValidationException($"Failed to assign role '{role}' to user: {message}");
            }
        }

        return user;
    }

    private static string NormalizeRole(string? requestedRole)
    {
        if (string.IsNullOrWhiteSpace(requestedRole))
            return RoleNames.Staff;

        var match = AllowedRoles.FirstOrDefault(r =>
            string.Equals(r, requestedRole, StringComparison.OrdinalIgnoreCase));

        if (match is null)
            throw new ValidationException("Role is invalid.");

        return match;
    }
}
