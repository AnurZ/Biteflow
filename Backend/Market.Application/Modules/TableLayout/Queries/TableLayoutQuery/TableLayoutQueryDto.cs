using Market.Domain.Common.Enums;
using MediatR;
using System.Collections.Generic;

namespace Market.Application.Modules.TableLayout.Querries.GetTableLayouts
{
    public sealed class GetTableLayoutsQuery : IRequest<List<TableLayoutDto>>
    {
        // Optional filter by layout name
        public string? Name { get; set; }
    }

    public sealed class TableLayoutDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? FloorImageUrl { get; set; }

        // List of tables in this layout
        public List<TableDto> Tables { get; set; } = new List<TableDto>();
    }

    public sealed class TableDto
    {
        public int Id { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int Number { get; set; }
        public int NumberOfSeats { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Shape { get; set; } = "rectangle";
        public string Color { get; set; } = "#00ff00";
        public TableTypes TableType { get; set; }
        public TableStatus Status { get; set; }
        public bool IsActive { get; set; }
    }
}
