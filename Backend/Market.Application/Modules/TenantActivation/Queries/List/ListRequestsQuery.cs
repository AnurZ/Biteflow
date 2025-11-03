using Market.Domain.Common.Enums;
using Market.Domain.Entities.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Queries.List
{
    public sealed record ListRequestsQuery(ActivationStatus? Status, int Page = 1, int PageSize = 20)
    : IRequest<PageResult<ActivationDraftDto>>;
}
