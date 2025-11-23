using Market.Application.Modules.MealCategory.Querries.GetMealCategories;
using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Querries.GetDiningTableList
{
    public sealed class GetDiningTableListQuery : IRequest<List<GetDiningTableListQueryDto>>
    {
        public string? SectionName { get; set; }
        public TableStatus? Status { get; set; }
        public int? MinimumSeats { get; set; }
    }
    public sealed class GetDiningTableListQueryDto
    {
        public int Id { get; set; }
        public string SectionName { get; set; }
        public int Number { get; set; }
        public int NumberOfSeats { get; set; }
        public TableTypes TableType { get; set; }
        public TableStatus Status { get; set; }
        public bool IsActive { get; set; }
    }
}
