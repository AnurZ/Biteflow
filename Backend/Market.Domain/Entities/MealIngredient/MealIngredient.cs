using Market.Domain.Common;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Meal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.MealIngredient
{
    public class MealIngredient:BaseEntity
    {
        public int MealId { get; set; }
        public Meal.Meal Meal { get; set; } = default!;
        public int InventoryItemId { get; set; }
        public InventoryItem.InventoryItem InventoryItem { get; set; } = default!;
        public double Quantity { get; set; }
        public UnitTypes UnitTypes { get; set; }
    }
}
