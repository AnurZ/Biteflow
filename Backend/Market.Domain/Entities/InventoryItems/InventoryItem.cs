using Market.Domain.Common;
using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.InventoryItem
{
    public class InventoryItem:BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public UnitTypes UnitType {  get; set; }
        public double ReorderQty {  get; set; }
        public int ReorderFrequency { get; set; }
        public double CurrentQty { get; set; }
    }
}
