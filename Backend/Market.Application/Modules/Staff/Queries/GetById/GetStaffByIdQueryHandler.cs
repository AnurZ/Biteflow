using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace Market.Application.Modules.Staff.Queries.GetById
{
    public sealed class GetStaffByIdQueryHandler(
        IAppDbContext db,
        UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetStaffByIdQuery, GetStaffByIdDto>
    {
        private static readonly string[] DisplayRoles =
        {
            RoleNames.Admin,
            RoleNames.Kitchen,
            RoleNames.Waiter
        };

        public async Task<GetStaffByIdDto> Handle(GetStaffByIdQuery req, CancellationToken ct)
        {
            var e = await db.EmployeeProfiles
                .AsNoTracking()
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

            if (e is null) throw new KeyNotFoundException("EmployeeProfile");

            return new GetStaffByIdDto
            {
                Id = e.Id,
                ApplicationUserId = e.ApplicationUserId,
                DisplayName = e.ApplicationUser?.DisplayName ?? string.Empty,
                Email = e.ApplicationUser?.Email ?? string.Empty,
                Role = await ResolveRoleAsync(e.ApplicationUser),
                Position = e.Position,
                FirstName = e.FirstName,
                LastName = e.LastName,
                PhoneNumber = e.PhoneNumber,
                HireDate = e.HireDate,
                TerminationDate = e.TerminationDate,
                Salary = e.Salary,
                HourlyRate = e.HourlyRate,
                EmploymentType = e.EmploymentType,
                ShiftType = e.ShiftType,
                ShiftStart = e.ShiftStart,
                ShiftEnd = e.ShiftEnd,
                AverageRating = e.AverageRating,
                CompletedOrders = e.CompletedOrders,
                MonthlyTips = e.MonthlyTips,
                IsActive = e.IsActive,
                Notes = e.Notes
            };
        }

        private async Task<string> ResolveRoleAsync(ApplicationUser? user)
        {
            if (user is null)
            {
                return string.Empty;
            }

            var roles = await userManager.GetRolesAsync(user);
            return DisplayRoles.FirstOrDefault(role =>
                roles.Contains(role, StringComparer.OrdinalIgnoreCase)) ?? string.Empty;
        }
    }
}
