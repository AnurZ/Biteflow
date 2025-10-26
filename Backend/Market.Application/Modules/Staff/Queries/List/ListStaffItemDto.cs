using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Market.Application.Modules.Staff.Queries.List
{
    public sealed class ListStaffItemDto
    {
        public int Id { get; init; }
        public int AppUserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Position { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime? HireDate { get; init; }
    }
}
