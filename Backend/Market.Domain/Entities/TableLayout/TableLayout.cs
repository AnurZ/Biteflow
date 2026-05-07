using Market.Domain.Common;
using Market.Domain.Entities.DiningTables;

namespace Market.Domain.Entities.TableLayout
{
    public class TableLayout : BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty; // Layout name, e.g., "Main Floor"
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? FloorImageUrl { get; set; }
        public ICollection<DiningTable> Tables { get; set; } = new List<DiningTable>();
    }
}
