using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.DiningTable.Querries.GetDiningTableTLIDbyTableID
{
    public class GetDiningTableTableLayoutIdByIdHandler
        : IRequestHandler<GetDiningTableTableLayoutIdByIdQuery, GetDiningTableTableLayoutIdByIdDto>
    {
        private readonly IAppDbContext _context;

        public GetDiningTableTableLayoutIdByIdHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<GetDiningTableTableLayoutIdByIdDto> Handle(
            GetDiningTableTableLayoutIdByIdQuery request,
            CancellationToken cancellationToken)
        {
            var tableLayoutId = await _context.DiningTables
                .Where(t => t.Id == request.DiningTableId)
                .Select(t => t.TableLayoutId)
                .FirstOrDefaultAsync(cancellationToken);

            if (tableLayoutId == 0)
                throw new KeyNotFoundException(
                    $"Dining table with ID {request.DiningTableId} not found."
                );

            return new GetDiningTableTableLayoutIdByIdDto
            {
                TableLayoutId = tableLayoutId
            };
        }
    }
}
