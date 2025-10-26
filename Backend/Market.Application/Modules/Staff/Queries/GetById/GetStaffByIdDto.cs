using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
namespace Market.Application.Modules.Staff.Queries.GetById
{
    public sealed class GetStaffByIdDto
    {
        public int Id { get; init; }
        public int AppUserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;

        public string Position { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; }
        public DateTime? HireDate { get; init; }
        public DateTime? TerminationDate { get; init; }
        public decimal? Salary { get; init; }
        public decimal? HourlyRate { get; init; }
        public string? EmploymentType { get; init; }
        public string? ShiftType { get; init; }
        public TimeOnly? ShiftStart { get; init; }
        public TimeOnly? ShiftEnd { get; init; }
        public double? AverageRating { get; init; }
        public int? CompletedOrders { get; init; }
        public decimal? MonthlyTips { get; init; }
        public bool IsActive { get; init; }
        public string? Notes { get; init; }
    }
}
