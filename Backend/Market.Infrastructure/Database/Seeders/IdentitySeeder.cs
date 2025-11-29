using Market.Domain.Entities.IdentityV2;
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
        await EnsureSuperAdminAsync(ct);
        await EnsureStaffProfilesAsync(ct);
    }

    private async Task EnsureRolesAsync(CancellationToken ct)
    {
        var roles = new[] { RoleNames.SuperAdmin, RoleNames.Admin, RoleNames.Staff, RoleNames.Customer };
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

    private async Task EnsureSuperAdminAsync(CancellationToken ct)
    {
        var seedSection = _configuration.GetSection("SeedAdmin");
        var email = seedSection["Email"];
        var password = seedSection["Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("SeedAdmin credentials not provided; skipping superadmin seeding.");
            return;
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = seedSection["DisplayName"] ?? "Super Administrator",
                TenantId = Guid.Empty,
                IsEnabled = true,
                EmailConfirmed = true,
            };

            var create = await _userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                _logger.LogError("Failed creating seed admin {Email}: {Errors}", email,
                    string.Join(", ", create.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Superadmin seeding failed.");
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(RoleNames.SuperAdmin))
        {
            await _userManager.AddToRoleAsync(user, RoleNames.SuperAdmin);
        }
    }

    private async Task EnsureStaffProfilesAsync(CancellationToken ct)
    {
        var staffUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Staff);
        foreach (var staff in staffUsers)
        {
            await _staffProfiles.EnsureProfileAsync(staff, ct);
        }

        var legacyStringUser = await EnsureLegacyIdentityUserAsync("string", "string", ct);
        if (legacyStringUser != null)
        {
            await EnsureRoleAsync(legacyStringUser, RoleNames.SuperAdmin, ct);
            await EnsureRoleAsync(legacyStringUser, RoleNames.Admin, ct);
            await EnsureRoleAsync(legacyStringUser, RoleNames.Staff, ct);
            await _staffProfiles.EnsureProfileAsync(legacyStringUser, ct);
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
}
