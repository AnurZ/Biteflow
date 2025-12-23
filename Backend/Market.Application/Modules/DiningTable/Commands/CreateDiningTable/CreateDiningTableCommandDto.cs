using Market.Domain.Common.Enums;
using MediatR;

namespace Market.Application.Modules.DiningTable.Commands.CreateDiningTable
{
    public sealed class CreateDiningTableCommandDto : IRequest<int>
    {
        // Basic info
        public int Number { get; set; }
        public int NumberOfSeats { get; set; }
        public TableTypes TableType { get; set; }

        // Layout and visual info
        public int TableLayoutId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; } = 50;
        public int Width { get; set; } = 50;
        public string Shape { get; set; } = "rectangle";
        public string Color { get; set; } = "#00ff00";

        // Optional status info
        public TableStatus Status { get; set; } = TableStatus.Free;
    }
}
