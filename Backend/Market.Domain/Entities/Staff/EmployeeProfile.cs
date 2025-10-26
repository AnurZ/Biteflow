using Market.Domain.Common;
using Market.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.Staff
{
    public class EmployeeProfile : BaseEntity
    {
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        public string Position { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public decimal? Salary { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? EmploymentType { get; set; } // Fulltime or partime
        public string? ShiftType { get; set; }       // Morning or evening shift
        public TimeOnly? ShiftStart { get; set; }
        public TimeOnly? ShiftEnd { get; set; }
        public double? AverageRating { get; set; }
        public int? CompletedOrders { get; set; }
        public decimal? MonthlyTips { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
}
