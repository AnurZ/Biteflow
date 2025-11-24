using Market.Domain.Common;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Orders;
using Market.Domain.Entities.TableReservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.DiningTables
{
    public class DiningTable : BaseEntity
    {
        public string SectionName { get; set; }
        public int Number { get; set; }
        public int NumberOfSeats { get; set; }
        public bool IsActive { get; set; }

        public TableTypes TableType { get; set; }
        public TableStatus Status { get; set; } = TableStatus.Free;
        public DateTime? LastUsedAt { get; set; }

        public ICollection<TableReservation> Reservations { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

}
