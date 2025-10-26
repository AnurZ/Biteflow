using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Staff.Queries.GetById
{
    public sealed class GetStaffByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetStaffByIdQuery, GetStaffByIdDto>
    {
        public async Task<GetStaffByIdDto> Handle(GetStaffByIdQuery req, CancellationToken ct)
        {
            var e = await db.EmployeeProfiles
                .Include(x => x.AppUser)
                .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

            if (e is null) throw new KeyNotFoundException("EmployeeProfile");

            return new GetStaffByIdDto
            {
                Id = e.Id,
                AppUserId = e.AppUserId,
                DisplayName = e.AppUser.DisplayName,
                Email = e.AppUser.Email,
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
    }
}
