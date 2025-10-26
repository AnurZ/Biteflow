using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Market.Application.Modules.Staff.Queries.GetById
{
    public sealed class GetStaffByIdQuery : IRequest<GetStaffByIdDto>
    {
        public int Id { get; init; }
    }
}
