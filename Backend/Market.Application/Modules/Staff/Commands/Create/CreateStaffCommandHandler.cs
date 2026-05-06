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
    private static readonly string[] AllAssignableRoles =
    {
        RoleNames.SuperAdmin,
        RoleNames.Admin,
        RoleNames.Staff,
        RoleNames.Waiter,
        RoleNames.Kitchen
    };

    private static readonly string[] RestaurantAdminAssignableRoles =
    {
        RoleNames.Staff,
        RoleNames.Waiter,
        RoleNames.Kitchen
    };

    private readonly IAppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateStaffCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public CreateStaffCommandHandler(
        IAppDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger<CreateStaffCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
        _tenantContext = tenantContext;
    }

    public async Task<int> Handle(CreateStaffCommand r, CancellationToken ct)
    {
        var tenantId = _tenantContext.RequireTenantId();
        var restaurantId = _tenantContext.RequireRestaurantId();

        if (string.IsNullOrWhiteSpace(r.FirstName) || string.IsNullOrWhiteSpace(r.LastName))
            throw new ValidationException("FirstName and LastName are required.");

        var targetRole = NormalizeRole(r.Role);
        EnsureRoleAllowedForCaller(targetRole);

        var email = r.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email is required when creating a new user.");

        var displayName = string.IsNullOrWhiteSpace(r.DisplayName)
            ? $"{r.FirstName} {r.LastName}".Trim()
            : r.DisplayName!.Trim();

        var plainPassword = string.IsNullOrWhiteSpace(r.PlainPassword)
            ? Guid.NewGuid().ToString("N")
            : r.PlainPassword!;

        var identityResult = await EnsureIdentityUserAsync(email!, displayName, plainPassword, tenantId, restaurantId, targetRole, ct);
        var identityUser = identityResult.User;

        var profile = new EmployeeProfile
        {
            TenantId = tenantId,
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
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            await CleanupIdentitySideEffectsAsync(identityResult, targetRole);
            throw;
        }

        return profile.Id;
    }

    private async Task<IdentityUserProvisionResult> EnsureIdentityUserAsync(
        string email,
        string displayName,
        string plainPassword,
        Guid tenantId,
        Guid restaurantId,
        string role,
        CancellationToken ct)
    {
        var normalizedEmail = email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        var createdUser = false;

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                DisplayName = displayName,
                TenantId = tenantId,
                RestaurantId = restaurantId,
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

            createdUser = true;
        }

        var addedRole = false;
        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var message = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new ValidationException($"Failed to assign role '{role}' to user: {message}");
            }

            addedRole = true;
        }

        return new IdentityUserProvisionResult(user, createdUser, addedRole);
    }

    private async Task CleanupIdentitySideEffectsAsync(IdentityUserProvisionResult result, string role)
    {
        if (result.CreatedUser)
        {
            var delete = await _userManager.DeleteAsync(result.User);
            if (!delete.Succeeded)
            {
                _logger.LogWarning("Failed to delete identity user {UserId} after staff profile creation failed: {Errors}",
                    result.User.Id,
                    string.Join(", ", delete.Errors.Select(e => e.Description)));
            }

            return;
        }

        if (result.AddedRole)
        {
            var removeRole = await _userManager.RemoveFromRoleAsync(result.User, role);
            if (!removeRole.Succeeded)
            {
                _logger.LogWarning("Failed to remove role {Role} from identity user {UserId} after staff profile creation failed: {Errors}",
                    role,
                    result.User.Id,
                    string.Join(", ", removeRole.Errors.Select(e => e.Description)));
            }
        }
    }

    private static string NormalizeRole(string? requestedRole)
    {
        if (string.IsNullOrWhiteSpace(requestedRole))
            return RoleNames.Staff;

        var match = AllAssignableRoles.FirstOrDefault(r =>
            string.Equals(r, requestedRole, StringComparison.OrdinalIgnoreCase));

        if (match is null)
            throw new ValidationException("Role is invalid.");

        return match;
    }

    private void EnsureRoleAllowedForCaller(string role)
    {
        if (_tenantContext.IsSuperAdmin)
        {
            return;
        }

        if (!RestaurantAdminAssignableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException("Role is not allowed for the current user.");
        }
    }

    private sealed record IdentityUserProvisionResult(
        ApplicationUser User,
        bool CreatedUser,
        bool AddedRole);
}
