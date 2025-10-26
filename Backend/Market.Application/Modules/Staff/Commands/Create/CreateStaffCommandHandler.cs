using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Entities.Staff;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Market.Application.Modules.Staff.Commands.Create
{
    public sealed class CreateStaffCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateStaffCommand, int>
    {
        public async Task<int> Handle(CreateStaffCommand r, CancellationToken ct)
        {

            if (string.IsNullOrWhiteSpace(r.FirstName) || string.IsNullOrWhiteSpace(r.LastName))
                throw new ValidationException("FirstName and LastName are required.");


            var userExists = await db.Users.AnyAsync(
                u => u.Id == r.AppUserId /* && u.TenantId == tenant.TenantId */, ct);

            if (!userExists) throw new ValidationException("AppUserId is invalid.");

            var e = new EmployeeProfile
            {
                AppUserId = r.AppUserId,
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

            db.EmployeeProfiles.Add(e);
            await db.SaveChangesAsync(ct);
            return e.Id;
        }
    }
}
