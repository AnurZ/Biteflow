using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
namespace Market.Application.Modules.Staff.Commands.Delete
{
    public sealed class DeleteStaffCommand : IRequest
    {
        public int Id { get; init; }
    }
}
