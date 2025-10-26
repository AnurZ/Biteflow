using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Market.Application.Modules.Staff.Queries.List
{
    public sealed class ListStaffQuery : BasePagedQuery<ListStaffItemDto>
    {
        public string? Search { get; init; }
        public string? Sort { get; init; }
    }
}
