using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.DiningTable.Queries.GetDiningTableTLIDbyTableID
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
            var table = await _context.DiningTables
                .Where(t => t.Id == request.DiningTableId)
                .Select(t => new
                {
                    t.TableLayoutId,
                    TableLayoutName = t.TableLayout.Name
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (table == null)
            {
                throw new KeyNotFoundException(
                    $"Dining table with ID {request.DiningTableId} not found."
                );
            }

            return new GetDiningTableTableLayoutIdByIdDto
            {
                TableLayoutId = table.TableLayoutId,
                TableLayoutName = table.TableLayoutName
            };
        }
    }
}
