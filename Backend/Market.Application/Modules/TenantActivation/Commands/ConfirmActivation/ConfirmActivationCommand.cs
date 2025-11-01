using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.ConfirmActivation
{
    public sealed record ConfirmActivationCommand(string token) : IRequest<Guid>;
}
