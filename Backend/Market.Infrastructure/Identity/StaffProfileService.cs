using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.Staff;
using Market.Infrastructure.Database;
using Market.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Market.Infrastructure.Identity;

public sealed class StaffProfileService
{
    private readonly DatabaseContext _db;
    private readonly ILogger<StaffProfileService> _logger;

    public StaffProfileService(
        DatabaseContext db,
        ILogger<StaffProfileService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> EnsureProfileAsync(ApplicationUser user, CancellationToken ct = default)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var effectiveTenantId = user.TenantId == Guid.Empty
            ? SeedConstants.DefaultTenantId
            : user.TenantId;
        var profile = await _db.EmployeeProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id, ct);

        if (profile != null)
        {
            if (profile.ApplicationUserId != user.Id)
            {
                profile.ApplicationUserId = user.Id;
                profile.TenantId = effectiveTenantId;
                await _db.SaveChangesAsync(ct);
            }
            return true;
        }

        var newProfile = new EmployeeProfile
        {
            ApplicationUserId = user.Id,
            TenantId = effectiveTenantId,
            Position = "Unassigned",
            FirstName = user.DisplayName ?? "Staff",
            LastName = string.Empty,
            IsActive = true
        };

        _db.EmployeeProfiles.Add(newProfile);

        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create staff profile for user {UserId}", user.Id);
            return false;
        }
    }
}
