using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Staff;
using Market.Infrastructure.Identity;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Market.Infrastructure.Database.Seeders;

public sealed class IdentitySeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly StaffProfileService _staffProfiles;
    private readonly DatabaseContext _db;
    private readonly ILogger<IdentitySeeder> _logger;
    private readonly IConfiguration _configuration;

    public IdentitySeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        StaffProfileService staffProfiles,
        DatabaseContext db,
        ILogger<IdentitySeeder> logger,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _staffProfiles = staffProfiles;
        _db = db;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureRolesAsync(ct);
        await EnsureConfiguredSeedAdminAsync(ct);
        await EnsureStaffProfilesAsync(ct);
        await EnsureExclusiveDemoSuperAdminAsync(ct);
    }

    private async Task EnsureRolesAsync(CancellationToken ct)
    {
        var roles = new[]
        {
            RoleNames.SuperAdmin,
            RoleNames.Admin,
            RoleNames.Staff,
            RoleNames.Waiter,
            RoleNames.Kitchen,
            RoleNames.Customer
        };
        foreach (var role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
                continue;

            var result = await _roleManager.CreateAsync(new ApplicationRole { Name = role });
            if (!result.Succeeded)
            {
                _logger.LogError("Failed creating role {Role}: {Errors}", role,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Role seeding failed.");
            }
        }
    }

    private async Task EnsureConfiguredSeedAdminAsync(CancellationToken ct)
    {
        var seedSection = _configuration.GetSection("SeedAdmin");
        var email = seedSection["Email"];
        var password = seedSection["Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("SeedAdmin credentials not provided; skipping seed admin seeding.");
            return;
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = seedSection["DisplayName"] ?? "Seed Administrator",
                TenantId = SeedConstants.DefaultTenantId,
                RestaurantId = SeedConstants.DefaultRestaurantId,
                IsEnabled = true,
                EmailConfirmed = true,
            };

            var create = await _userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                _logger.LogError("Failed creating seed admin {Email}: {Errors}", email,
                    string.Join(", ", create.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Seed admin seeding failed.");
            }
        }
        else
        {
            await EnsureDemoRestaurantContextAsync(user, ct);
        }

        await EnsureRoleAsync(user, RoleNames.Admin, ct);
        await RemoveRoleIfPresentAsync(user, RoleNames.SuperAdmin, ct);
    }

    private async Task EnsureStaffProfilesAsync(CancellationToken ct)
    {
        var staffRoles = new[] { RoleNames.Staff, RoleNames.Waiter, RoleNames.Kitchen };
        foreach (var staffRole in staffRoles)
        {
            var users = await _userManager.GetUsersInRoleAsync(staffRole);
            foreach (var user in users)
            {
                await _staffProfiles.EnsureProfileAsync(user, ct);
            }
        }

        var employeeProfileChanged = false;

        employeeProfileChanged |= await EnsureDemoUserAsync(
            username: "string",
            password: "StringUser1!",
            primaryRole: RoleNames.Admin,
            position: "Manager",
            firstName: "Admin",
            lastName: "User",
            ct: ct);

        employeeProfileChanged |= await EnsureDemoUserAsync(
            username: "waiter1",
            password: "WaiterUser1!",
            primaryRole: RoleNames.Waiter,
            position: "Waiter",
            firstName: "Waiter",
            lastName: "One",
            ct: ct);

        employeeProfileChanged |= await EnsureDemoUserAsync(
            username: "kitchen1",
            password: "KitchenUser1!",
            primaryRole: RoleNames.Kitchen,
            position: "Kitchen",
            firstName: "Kitchen",
            lastName: "One",
            ct: ct);

        if (employeeProfileChanged)
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<bool> EnsureDemoUserAsync(
        string username,
        string password,
        string primaryRole,
        string position,
        string firstName,
        string lastName,
        CancellationToken ct)
    {
        var identityUser = await EnsureDemoIdentityUserAsync(username, password);
        if (identityUser == null)
            return false;

        await EnsurePasswordAsync(identityUser, password, ct);
        await EnsureRoleAsync(identityUser, primaryRole, ct);

        var managedRoles = new[]
        {
            RoleNames.SuperAdmin,
            RoleNames.Admin,
            RoleNames.Staff,
            RoleNames.Waiter,
            RoleNames.Kitchen,
            RoleNames.Customer
        };

        foreach (var role in managedRoles.Where(r => !string.Equals(r, primaryRole, StringComparison.OrdinalIgnoreCase)))
        {
            await RemoveRoleIfPresentAsync(identityUser, role, ct);
        }

        return await EnsureEmployeeProfileAsync(identityUser, position, firstName, lastName, ct);
    }

    private async Task<bool> EnsureEmployeeProfileAsync(
        ApplicationUser identityUser,
        string position,
        string firstName,
        string lastName,
        CancellationToken ct)
    {
        var effectiveTenantId = identityUser.TenantId == Guid.Empty
            ? SeedConstants.DefaultTenantId
            : identityUser.TenantId;

        var profile = await _db.EmployeeProfiles
            // Identity seeding repairs demo profiles regardless of the current request tenant.
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ApplicationUserId == identityUser.Id, ct);

        if (profile == null)
        {
            profile = new EmployeeProfile
            {
                ApplicationUserId = identityUser.Id,
                TenantId = effectiveTenantId,
                Position = position,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            _db.EmployeeProfiles.Add(profile);
            return true;
        }

        var changed = false;
        if (profile.ApplicationUserId != identityUser.Id)
        {
            profile.ApplicationUserId = identityUser.Id;
            changed = true;
        }

        if (profile.TenantId != effectiveTenantId)
        {
            profile.TenantId = effectiveTenantId;
            changed = true;
        }

        if (!string.Equals(profile.Position, position, StringComparison.Ordinal))
        {
            profile.Position = position;
            changed = true;
        }

        if (!string.Equals(profile.FirstName, firstName, StringComparison.Ordinal))
        {
            profile.FirstName = firstName;
            changed = true;
        }

        if (!string.Equals(profile.LastName, lastName, StringComparison.Ordinal))
        {
            profile.LastName = lastName;
            changed = true;
        }

        if (!profile.IsActive)
        {
            profile.IsActive = true;
            changed = true;
        }

        return changed;
    }

    // Dedicated demo platform owner account.
    // Intended credentials: superadmin / Superadmin1!
    // Intended role set: superadmin only (exclusive).
    private async Task EnsureExclusiveDemoSuperAdminAsync(CancellationToken ct)
    {
        const string username = "superadmin";
        const string email = "superadmin@demo.local";
        const string password = "Superadmin1!";

        var demoSuperAdmin = await _userManager.FindByNameAsync(username)
            ?? await _userManager.FindByEmailAsync(email);

        if (demoSuperAdmin == null)
        {
            demoSuperAdmin = new ApplicationUser
            {
                UserName = username,
                Email = email,
                DisplayName = "Super Administrator",
                TenantId = Guid.Empty,
                IsEnabled = true,
                EmailConfirmed = true
            };

            var create = await _userManager.CreateAsync(demoSuperAdmin, password);
            if (!create.Succeeded)
            {
                _logger.LogError("Failed creating demo superadmin '{Username}': {Errors}", username,
                    string.Join(", ", create.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Demo superadmin seeding failed.");
            }
        }
        else
        {
            var changed = false;

            if (!string.Equals(demoSuperAdmin.UserName, username, StringComparison.OrdinalIgnoreCase))
            {
                demoSuperAdmin.UserName = username;
                changed = true;
            }

            if (!string.Equals(demoSuperAdmin.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                demoSuperAdmin.Email = email;
                changed = true;
            }

            if (!demoSuperAdmin.IsEnabled)
            {
                demoSuperAdmin.IsEnabled = true;
                changed = true;
            }

            if (!demoSuperAdmin.EmailConfirmed)
            {
                demoSuperAdmin.EmailConfirmed = true;
                changed = true;
            }

            if (changed)
            {
                var update = await _userManager.UpdateAsync(demoSuperAdmin);
                if (!update.Succeeded)
                {
                    _logger.LogError("Failed updating demo superadmin '{Username}': {Errors}", username,
                        string.Join(", ", update.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException("Demo superadmin update failed.");
                }
            }

            await EnsurePasswordAsync(demoSuperAdmin, password, ct);
        }

        await EnsureRoleAsync(demoSuperAdmin, RoleNames.SuperAdmin, ct);

        var superAdmins = await _userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin);
        foreach (var user in superAdmins.Where(x => x.Id != demoSuperAdmin.Id))
        {
            await RemoveRoleIfPresentAsync(user, RoleNames.SuperAdmin, ct);
        }
    }

    private async Task<ApplicationUser?> EnsureDemoIdentityUserAsync(string username, string defaultPassword)
    {
        var identityUser = await _userManager.FindByNameAsync(username)
            ?? await _userManager.FindByEmailAsync(username);

        var normalizedEmail = username.Contains("@", StringComparison.Ordinal)
            ? username
            : $"{username}@legacy.local";

        if (identityUser != null)
        {
            var changed = false;

            if (!string.Equals(identityUser.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                identityUser.Email = normalizedEmail;
                identityUser.NormalizedEmail = normalizedEmail.ToUpperInvariant();
                changed = true;
            }

            if (identityUser.TenantId == Guid.Empty)
            {
                identityUser.TenantId = SeedConstants.DefaultTenantId;
                changed = true;
            }

            if (identityUser.RestaurantId is null || identityUser.RestaurantId == Guid.Empty)
            {
                identityUser.RestaurantId = SeedConstants.DefaultRestaurantId;
                changed = true;
            }

            if (changed)
            {
                var update = await _userManager.UpdateAsync(identityUser);
                if (!update.Succeeded)
                {
                    _logger.LogWarning("Failed updating demo identity user {Username}: {Errors}", username,
                        string.Join(", ", update.Errors.Select(e => e.Description)));
                }
            }
            return identityUser;
        }

        identityUser = new ApplicationUser
        {
            UserName = username,
            Email = normalizedEmail,
            DisplayName = username,
            TenantId = SeedConstants.DefaultTenantId,
            RestaurantId = SeedConstants.DefaultRestaurantId,
            IsEnabled = true,
            EmailConfirmed = true
        };

        var create = await _userManager.CreateAsync(identityUser, defaultPassword);
        if (!create.Succeeded)
        {
            _logger.LogWarning("Failed creating demo identity user {Username}: {Errors}", username,
                string.Join(", ", create.Errors.Select(e => e.Description)));
            return null;
        }

        return identityUser;
    }

    private async Task EnsureDemoRestaurantContextAsync(ApplicationUser user, CancellationToken ct)
    {
        var changed = false;

        if (user.TenantId == Guid.Empty)
        {
            user.TenantId = SeedConstants.DefaultTenantId;
            changed = true;
        }

        if (user.RestaurantId is null || user.RestaurantId == Guid.Empty)
        {
            user.RestaurantId = SeedConstants.DefaultRestaurantId;
            changed = true;
        }

        if (!changed)
            return;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            _logger.LogWarning("Failed updating restaurant context for user {UserId}: {Errors}",
                user.Id, string.Join(", ", update.Errors.Select(e => e.Description)));
        }
    }

    private async Task EnsureRoleAsync(ApplicationUser user, string role, CancellationToken ct)
    {
        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed adding role {Role} to user {UserId}: {Errors}",
                    role, user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task RemoveRoleIfPresentAsync(ApplicationUser user, string role, CancellationToken ct)
    {
        if (!await _userManager.IsInRoleAsync(user, role))
            return;

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed removing role {Role} from user {UserId}: {Errors}",
                role, user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task EnsurePasswordAsync(ApplicationUser user, string desiredPassword, CancellationToken ct)
    {
        if (await _userManager.CheckPasswordAsync(user, desiredPassword))
            return;

        if (await _userManager.HasPasswordAsync(user))
        {
            var remove = await _userManager.RemovePasswordAsync(user);
            if (!remove.Succeeded)
            {
                _logger.LogWarning("Failed removing password for user {UserId}: {Errors}",
                    user.Id, string.Join(", ", remove.Errors.Select(e => e.Description)));
                return;
            }
        }

        var add = await _userManager.AddPasswordAsync(user, desiredPassword);
        if (!add.Succeeded)
        {
            _logger.LogWarning("Failed setting password for user {UserId}: {Errors}",
                user.Id, string.Join(", ", add.Errors.Select(e => e.Description)));
        }
    }
}
