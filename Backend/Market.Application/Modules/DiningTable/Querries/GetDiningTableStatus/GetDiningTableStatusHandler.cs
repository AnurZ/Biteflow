using Market.Application.Modules.DiningTable.Querries.GetDiningTableStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

internal sealed class GetDiningTablesHandler
    : IRequestHandler<GetDiningTablesStatusQuery, List<GetDiningTableStatusDto>>
{
    private readonly IAppDbContext _db;

    public GetDiningTablesHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<GetDiningTableStatusDto>> Handle(
        GetDiningTablesStatusQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.DiningTables.AsQueryable();

        if (request.TableLayoutId.HasValue)
        {
            query = query.Where(t => t.TableLayoutId == request.TableLayoutId);
        }

        return await query
            .Select(t => new GetDiningTableStatusDto
            {
                Id = t.Id,
                TableLayoutId = t.TableLayoutId,
                Status = t.Status
            })
            .ToListAsync(cancellationToken);
    }
}
