using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.Tenants
{
    public sealed record ActivationDraftDto(
    int Id,
    string RestaurantName,
    string Domain,
    string OwnerFullName,
    string OwnerEmail,
    string OwnerPhone,
    string Address,
    string City,
    string State,
    ActivationStatus Status);
}
