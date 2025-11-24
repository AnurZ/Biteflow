using Market.Domain.Common.Enums;

namespace Market.Application.Modules.DiningTable.Commands.CreateDiningTable
{
    public sealed class CreateDiningTableCommandDto:IRequest<int>
    {
        public string SectionName { get; set; }
        public int Number { get; set; }
        public int NumberOfSeats { get; set; }
        public TableTypes TableType { get; set; }
    }
}
