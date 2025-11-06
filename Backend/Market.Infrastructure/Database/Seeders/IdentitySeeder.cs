using Market.Domain.Entities.IdentityV2;
using Market.Infrastructure.Identity;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Market.Infrastructure.Database.Seeders;

public sealed class IdentitySeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly StaffProfileService _staffProfiles;
    private readonly ILogger<IdentitySeeder> _logger;
    private readonly IConfiguration _configuration;

    public IdentitySeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        StaffProfileService staffProfiles,
        ILogger<IdentitySeeder> logger,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _staffProfiles = staffProfiles;
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
        var roles = new[] { RoleNames.SuperAdmin, RoleNames.Admin, RoleNames.Staff };
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
    }
}
