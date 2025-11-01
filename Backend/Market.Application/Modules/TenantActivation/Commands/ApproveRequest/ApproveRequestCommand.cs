using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.ApproveRequest
{
    public sealed record ApproveRequestCommand(int Id) : IRequest<string>;
}
