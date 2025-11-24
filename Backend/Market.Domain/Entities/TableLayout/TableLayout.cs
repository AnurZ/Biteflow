using Market.Domain.Entities.DiningTables;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.TableLayout
{
    public class TableLayout
    {
        public int Id { get; set; }
        public string Name { get; set; } // Layout name, e.g., "Main Floor"
        public string BackgroundColor { get; set; } = "#ffffff";
        public string FloorImageUrl { get; set; }
        public ICollection<DiningTable> Tables { get; set; }
    }
}
