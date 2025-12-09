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
            public int? TableLayoutId { get; set; } // optional filter by layout
            public int? Id { get; set; }
            public int? Number {  get; set; }
        }

        public sealed class GetDiningTableListQueryDto
        {
            public int Id { get; set; }
            public int TableLayoutId { get; set; } // new property
            public string SectionName { get; set; }
            public int Number { get; set; }
            public int NumberOfSeats { get; set; }
            public TableTypes TableType { get; set; }
            public TableStatus Status { get; set; }
            public bool IsActive { get; set; }

            // Position and visual properties
            public int X { get; set; }
            public int Y { get; set; }
            public int TableSize { get; set; }
            public string Shape { get; set; }
            public string Color { get; set; }

            public DateTime? LastUsedAt { get; set; }
        }
    }

