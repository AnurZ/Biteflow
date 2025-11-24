using Market.Domain.Common;
using Market.Domain.Entities.DiningTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.Orders
{
    public class Order : BaseEntity
    {

        public int? DiningTableId { get; set; }  // nullable → eat-out order
        public DiningTable? DiningTable { get; set; }

    }
}
