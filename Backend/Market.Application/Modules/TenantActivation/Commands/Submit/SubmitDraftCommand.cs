using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Submit
{
    public sealed record SubmitDraftCommand(int Id) : IRequest;
}
