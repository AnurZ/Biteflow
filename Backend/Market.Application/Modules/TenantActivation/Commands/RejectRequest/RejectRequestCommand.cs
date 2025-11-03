using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.RejectRequest
{
    public sealed record RejectRequestCommand(int Id, string Reason) : IRequest;
}
