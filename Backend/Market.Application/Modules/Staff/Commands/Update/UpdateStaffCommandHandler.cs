using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace Market.Application.Modules.Staff.Commands.Update
{
    public sealed class UpdateStaffCommandHandler(
        IAppDbContext db,
        ITenantContext tenantContext,
        UserManager<ApplicationUser> userManager)
    : IRequestHandler<UpdateStaffCommand>
    {
        private static readonly string[] ActiveStaffRoles =
        {
            RoleNames.Admin,
            RoleNames.Waiter,
            RoleNames.Kitchen
        };

        private static readonly string[] ManagedStaffRoles =
        {
            RoleNames.Admin,
            RoleNames.Staff,
            RoleNames.Waiter,
            RoleNames.Kitchen
        };

        public async Task Handle(UpdateStaffCommand r, CancellationToken ct)
        {
            var e = await db.EmployeeProfiles
                .WhereTenantOwned(tenantContext)
                .FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e is null) throw new KeyNotFoundException("EmployeeProfile");

            var identityUser = await userManager.FindByIdAsync(e.ApplicationUserId.ToString());
            if (identityUser is null)
                throw new MarketNotFoundException("User not found for staff.");

            var targetRole = string.IsNullOrWhiteSpace(r.Role)
                ? await ResolveCurrentRoleAsync(identityUser)
                : NormalizeRole(r.Role);

            EnsureRoleAllowedForCaller(targetRole);
            e.Position = ResolvePosition(targetRole);
            e.FirstName = r.FirstName.Trim();
            e.LastName = r.LastName.Trim();
            e.PhoneNumber = r.PhoneNumber;
            e.HireDate = r.HireDate;
            e.TerminationDate = r.TerminationDate;
            e.Salary = r.Salary;
            e.HourlyRate = r.HourlyRate;
            e.EmploymentType = r.EmploymentType;
            e.ShiftType = r.ShiftType;
            e.ShiftStart = r.ShiftStart;
            e.ShiftEnd = r.ShiftEnd;
            e.IsActive = r.IsActive;
            e.Notes = r.Notes;

            if (!string.IsNullOrWhiteSpace(r.DisplayName))
            {
                identityUser.DisplayName = r.DisplayName.Trim();
                var update = await userManager.UpdateAsync(identityUser);
                if (!update.Succeeded)
                {
                    var message = string.Join(", ", update.Errors.Select(e => e.Description));
                    throw new ValidationException($"Failed to update identity user: {message}");
                }
            }

            if (!string.IsNullOrWhiteSpace(r.Role))
            {
                await ReplaceStaffRoleAsync(identityUser, targetRole);
            }

            await db.SaveChangesAsync(ct);
        }

        private static string NormalizeRole(string requestedRole)
        {
            var match = ActiveStaffRoles.FirstOrDefault(r =>
                string.Equals(r, requestedRole, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                throw new ValidationException("Role is invalid.");

            return match;
        }

        private void EnsureRoleAllowedForCaller(string role)
        {
            if (tenantContext.IsSuperAdmin)
            {
                return;
            }

            if (!ActiveStaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                throw new ValidationException("Role is not allowed for the current user.");
            }
        }

        private async Task ReplaceStaffRoleAsync(ApplicationUser user, string targetRole)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            var managedRoles = currentRoles
                .Where(role => ManagedStaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .Where(role => !string.Equals(role, targetRole, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (managedRoles.Length > 0)
            {
                var remove = await userManager.RemoveFromRolesAsync(user, managedRoles);
                if (!remove.Succeeded)
                {
                    var message = string.Join(", ", remove.Errors.Select(e => e.Description));
                    throw new ValidationException($"Failed to remove existing staff roles: {message}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, targetRole))
            {
                var add = await userManager.AddToRoleAsync(user, targetRole);
                if (!add.Succeeded)
                {
                    var message = string.Join(", ", add.Errors.Select(e => e.Description));
                    throw new ValidationException($"Failed to assign role '{targetRole}' to user: {message}");
                }
            }
        }

        private async Task<string> ResolveCurrentRoleAsync(ApplicationUser user)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            return ActiveStaffRoles.FirstOrDefault(role =>
                currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase)) ?? RoleNames.Admin;
        }

        private static string ResolvePosition(string role)
        {
            return role.ToLowerInvariant() switch
            {
                RoleNames.Admin => "Manager",
                RoleNames.Waiter => "Waiter",
                RoleNames.Kitchen => "Cook",
                _ => "Manager"
            };
        }
    }
}
