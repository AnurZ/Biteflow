using Market.Domain.Common;
using Market.Domain.Entities.MealCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.Meal
{
    public class Meal:BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double BasePrice { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public string ImageField { get; set; } = string.Empty;
        public bool StockManaged { get; set; }
        public int? CategoryId { get; set; }
        public MealCategory.MealCategory? Category { get; set; } = default!;
        public ICollection<MealIngredient.MealIngredient> Ingredients { get; set; } = new List<MealIngredient.MealIngredient>();

    }
}
