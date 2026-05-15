using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.DataImport.OrderImport
{
    public sealed class OrderImportRowDto
    {
        public int? OrderId { get; set; }

        public string Status { get; set; }

        public int? DiningTableId { get; set; }

        public int? TableNumber { get; set; }

        public string? Notes { get; set; }
    }
}
