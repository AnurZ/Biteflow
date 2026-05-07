using Duende.IdentityServer.Stores;
using Market.Application.Abstractions;
using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace Market.API.Identity;

public sealed class StaffIdentityTerminationService(
    UserManager<ApplicationUser> userManager,
    IPersistedGrantStore persistedGrantStore,
    ILogger<StaffIdentityTerminationService> logger) : IStaffIdentityTerminationService
{
    private const string RefreshTokenGrantType = "refresh_token";

    private static readonly string[] StaffBoundRoles =
    {
        RoleNames.Admin,
        RoleNames.Staff,
        RoleNames.Waiter,
        RoleNames.Kitchen
    };

    public async Task TerminateAsync(Guid applicationUserId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(applicationUserId.ToString());
        if (user is null)
        {
            throw new ValidationException("Identity user for staff profile was not found.");
        }

        user.IsEnabled = false;
        user.LockoutEnabled = true;

        await EnsureIdentitySuccessAsync(
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue),
            "Failed to lock out staff identity user.");

        await EnsureIdentitySuccessAsync(
            await userManager.UpdateAsync(user),
            "Failed to disable staff identity user.");

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles
            .Where(role => StaffBoundRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            await EnsureIdentitySuccessAsync(
                await userManager.RemoveFromRolesAsync(user, rolesToRemove),
                "Failed to remove staff roles from identity user.");
        }

        await persistedGrantStore.RemoveAllAsync(new PersistedGrantFilter
        {
            SubjectId = user.Id.ToString(),
            Type = RefreshTokenGrantType
        });

        logger.LogInformation(
            "Terminated staff identity user {UserId}. RemovedRoles={Roles}",
            user.Id,
            string.Join(", ", rolesToRemove));
    }

    private static Task EnsureIdentitySuccessAsync(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return Task.CompletedTask;
        }

        var details = string.Join(", ", result.Errors.Select(x => x.Description));
        throw new ValidationException($"{message} {details}");
    }
}
