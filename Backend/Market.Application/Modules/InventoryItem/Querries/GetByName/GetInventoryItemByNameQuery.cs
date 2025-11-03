using Market.Application.Abstractions;
using Market.Application.Modules.InventoryItem.Querries.GetByName;
using MediatR;

namespace Market.Application.Modules.InventoryItem.Queries.GetByName
{
    public sealed class GetInventoryItemByNameQuery : BasePagedQuery<GetInventoryItemByNameDto>
    {
        public string Name { get; init; } = string.Empty;
    }
}
