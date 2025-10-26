using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
namespace Market.Application.Modules.Staff.Commands.Create
{
    public sealed class CreateStaffCommand : IRequest<int>
    {
        public int AppUserId { get; init; }
        public string Position { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; }
        public DateTime? HireDate { get; init; }
        public decimal? HourlyRate { get; init; }
        public string? EmploymentType { get; init; }
        public string? ShiftType { get; init; }
        public TimeOnly? ShiftStart { get; init; }
        public TimeOnly? ShiftEnd { get; init; }
        public bool IsActive { get; init; } = true;
        public string? Notes { get; init; }
    }
}
