using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.InventoryItem.Commands.Update
{
    public sealed class UpdateInventoryItemCommand : IRequest
    {
        public int Id { get; set; }
        public Guid RestaurantId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public UnitTypes UnitType { get; init; }
        public double ReorderQty { get; init; }
        public int ReorderFrequency { get; init; }
        public double CurrentQty { get; init; }
    }
}
