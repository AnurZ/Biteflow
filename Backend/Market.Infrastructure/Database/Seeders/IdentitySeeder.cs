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
    private readonly DatabaseContext _legacyContext;
    private readonly ILogger<IdentitySeeder> _logger;
    private readonly IConfiguration _configuration;

    public IdentitySeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        StaffProfileService staffProfiles,
        DatabaseContext legacyContext,
        ILogger<IdentitySeeder> logger,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _staffProfiles = staffProfiles;
        _legacyContext = legacyContext;
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
                TenantId = Guid.Empty,
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

        await EnsureDemoUserAsync(
            username: "string",
            password: "string",
            primaryRole: RoleNames.Admin,
            position: "Manager",
            firstName: "Admin",
            lastName: "User",
            ct: ct);

        await EnsureDemoUserAsync(
            username: "waiter1",
            password: "waiter1",
            primaryRole: RoleNames.Waiter,
            position: "Waiter",
            firstName: "Waiter",
            lastName: "One",
            ct: ct);

        await EnsureDemoUserAsync(
            username: "kitchen1",
            password: "kitchen1",
            primaryRole: RoleNames.Kitchen,
            position: "Kitchen",
            firstName: "Kitchen",
            lastName: "One",
            ct: ct);
    }

    private async Task EnsureDemoUserAsync(
        string username,
        string password,
        string primaryRole,
        string position,
        string firstName,
        string lastName,
        CancellationToken ct)
    {
        var identityUser = await EnsureLegacyIdentityUserAsync(username, password, ct);
        if (identityUser == null)
            return;

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

        await EnsureLegacyEmployeeProfileAsync(identityUser, username, position, firstName, lastName, ct);
    }

    private async Task EnsureLegacyEmployeeProfileAsync(
        ApplicationUser identityUser,
        string legacyEmailOrUsername,
        string position,
        string firstName,
        string lastName,
        CancellationToken ct)
    {
        var normalized = legacyEmailOrUsername.Trim().ToLowerInvariant();

        var legacyUser = await _legacyContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalized, ct);

        if (legacyUser == null)
        {
            _logger.LogWarning("Legacy AppUser '{Email}' not found while seeding profile for identity user {UserId}.",
                legacyEmailOrUsername, identityUser.Id);
            return;
        }

        var effectiveTenantId = legacyUser.TenantId == Guid.Empty
            ? SeedConstants.DefaultTenantId
            : legacyUser.TenantId;

        var profile = await _legacyContext.EmployeeProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.AppUserId == legacyUser.Id || x.ApplicationUserId == identityUser.Id, ct);

        if (profile == null)
        {
            profile = new EmployeeProfile
            {
                AppUserId = legacyUser.Id,
                ApplicationUserId = identityUser.Id,
                TenantId = effectiveTenantId,
                Position = position,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            _legacyContext.EmployeeProfiles.Add(profile);
            await _legacyContext.SaveChangesAsync(ct);
            return;
        }

        var changed = false;
        if (profile.AppUserId != legacyUser.Id)
        {
            profile.AppUserId = legacyUser.Id;
            changed = true;
        }

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

        if (changed)
        {
            await _legacyContext.SaveChangesAsync(ct);
        }
    }

    // Dedicated demo platform owner account.
    // Intended credentials: superadmin / superadmin.
    // Intended role set: superadmin only (exclusive).
    private async Task EnsureExclusiveDemoSuperAdminAsync(CancellationToken ct)
    {
        const string username = "superadmin";
        const string email = "superadmin@demo.local";
        const string password = "superadmin";

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

    private async Task<ApplicationUser?> EnsureLegacyIdentityUserAsync(string email, string defaultPassword, CancellationToken ct)
    {
        var identityUser = await _userManager.FindByNameAsync(email)
            ?? await _userManager.FindByEmailAsync(email);

        var normalizedEmail = email.Contains("@", StringComparison.Ordinal)
            ? email
            : $"{email}@legacy.local";

        if (identityUser != null)
        {
            if (!string.Equals(identityUser.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                identityUser.Email = normalizedEmail;
                identityUser.NormalizedEmail = normalizedEmail.ToUpperInvariant();
                var update = await _userManager.UpdateAsync(identityUser);
                if (!update.Succeeded)
                {
                    _logger.LogWarning("Failed updating legacy identity user {Email}: {Errors}", email,
                        string.Join(", ", update.Errors.Select(e => e.Description)));
                }
            }
            return identityUser;
        }

        var legacyUser = await _legacyContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), ct);

        identityUser = new ApplicationUser
        {
            UserName = email,
            Email = normalizedEmail,
            DisplayName = legacyUser?.DisplayName ?? email,
            TenantId = legacyUser?.TenantId ?? Guid.Empty,
            IsEnabled = true,
            EmailConfirmed = true
        };

        var create = await _userManager.CreateAsync(identityUser, defaultPassword);
        if (!create.Succeeded)
        {
            _logger.LogWarning("Failed creating legacy identity user {Email}: {Errors}", email,
                string.Join(", ", create.Errors.Select(e => e.Description)));
            return null;
        }

        return identityUser;
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
