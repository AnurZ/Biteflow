using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Create
{
    public sealed record CreateDraftCommand(
    string RestaurantName,
    string Domain,
    string OwnerFullName,
    string OwnerEmail,
    string OwnerPhone,
    string Address,
    string City,
    string State) : IRequest<int>;
}
