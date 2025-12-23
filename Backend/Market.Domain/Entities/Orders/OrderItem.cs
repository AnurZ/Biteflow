using Market.Domain.Common;

namespace Market.Domain.Entities.Orders
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int? MealId { get; set; }
        public Market.Domain.Entities.Meal.Meal? Meal { get; set; }

        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
