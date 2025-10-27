using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Market.Application.Modules.Staff.Commands.Update
{
    public sealed class UpdateStaffCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateStaffCommand>
    {
        public async Task Handle(UpdateStaffCommand r, CancellationToken ct)
        {
            var e = await db.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (e is null) throw new KeyNotFoundException("EmployeeProfile");

            e.Position = r.Position.Trim();
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
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == e.AppUserId, ct)
                           ?? throw new MarketNotFoundException("User not found for staff.");
                user.DisplayName = r.DisplayName.Trim();
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
