using Market.Domain.Entities.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Queries.GetById
{
    public sealed record GetTenantActivationByIdRequestQuery(int Id) : IRequest<ActivationDraftDto>;
}
