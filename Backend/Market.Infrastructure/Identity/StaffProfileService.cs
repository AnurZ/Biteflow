using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Staff;
using Market.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Market.Infrastructure.Identity;

public sealed class StaffProfileService
{
    private readonly DatabaseContext _legacyContext;
    private readonly ILogger<StaffProfileService> _logger;

    public StaffProfileService(
        DatabaseContext legacyContext,
        ILogger<StaffProfileService> logger)
    {
        _legacyContext = legacyContext;
        _logger = logger;
    }

    public async Task<bool> EnsureProfileAsync(ApplicationUser user, CancellationToken ct = default)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var profile = await _legacyContext.EmployeeProfiles
            .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id, ct);

        if (profile != null)
        {
            if (profile.ApplicationUserId != user.Id)
            {
                profile.ApplicationUserId = user.Id;
                profile.TenantId = user.TenantId;
                await _legacyContext.SaveChangesAsync(ct);
            }
            return true;
        }

        var legacyAppUser = await _legacyContext.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == user.Email.ToLower(), ct);

        if (legacyAppUser != null)
        {
            profile = await _legacyContext.EmployeeProfiles
                .FirstOrDefaultAsync(x => x.AppUserId == legacyAppUser.Id, ct);

            if (profile != null)
            {
                profile.ApplicationUserId = user.Id;
                profile.TenantId = user.TenantId;
                profile.FirstName = user.DisplayName ?? profile.FirstName;
                await _legacyContext.SaveChangesAsync(ct);
                return true;
            }
        }

        var newProfile = new EmployeeProfile
        {
            ApplicationUserId = user.Id,
            TenantId = user.TenantId,
            Position = "Unassigned",
            FirstName = user.DisplayName ?? "Staff",
            LastName = string.Empty,
            IsActive = true
        };

        _legacyContext.EmployeeProfiles.Add(newProfile);

        try
        {
            await _legacyContext.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create staff profile for user {UserId}", user.Id);
            return false;
        }
    }
}
