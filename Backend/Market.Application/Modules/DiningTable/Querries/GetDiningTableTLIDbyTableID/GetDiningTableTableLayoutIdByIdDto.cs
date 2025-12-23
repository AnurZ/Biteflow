using MediatR;

namespace Market.Application.Modules.DiningTable.Querries.GetDiningTableTLIDbyTableID

{
    public class GetDiningTableTableLayoutIdByIdQuery
        : IRequest<GetDiningTableTableLayoutIdByIdDto>
    {
        public int DiningTableId { get; set; }
    }

    public class GetDiningTableTableLayoutIdByIdDto
    {
        public int TableLayoutId { get; set; }
    }
}
