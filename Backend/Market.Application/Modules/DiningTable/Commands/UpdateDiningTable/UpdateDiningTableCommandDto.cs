using Market.Domain.Common.Enums;
using MediatR;
using System;

namespace Market.Application.Modules.DiningTable.Commands.UpdateDiningTable
{
    public sealed class UpdateDiningTableCommandDto : IRequest
    {
        public int Id { get; set; }

        public int Number { get; set; }
        public int NumberOfSeats { get; set; }
        public bool IsActive { get; set; }

        // Layout and visual info
        public int TableLayoutId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; } = 50;
        public int Width { get; set; } = 50;
        public string Shape { get; set; } = "rectangle";
        public string Color { get; set; } = "#00ff00";

        // Status info
        public TableTypes TableType { get; set; }
        public TableStatus Status { get; set; } = TableStatus.Free;
        public DateTime? LastUsedAt { get; set; }
    }
}
