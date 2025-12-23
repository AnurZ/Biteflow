using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Querries.GetDiningTableStatus
{
    public sealed class GetDiningTablesStatusQuery
    : IRequest<List<GetDiningTableStatusDto>>
    {
        public int? TableLayoutId { get; init; }
    }
    public sealed class GetDiningTableStatusDto
    {
        public int Id { get; set; }
        public int TableLayoutId { get; set; }
        public TableStatus Status { get; set; } 
    }
}
