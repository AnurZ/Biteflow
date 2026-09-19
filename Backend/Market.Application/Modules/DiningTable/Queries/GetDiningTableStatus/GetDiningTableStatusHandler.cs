using Market.Application.Modules.DiningTable.Queries.GetDiningTableStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

internal sealed class GetDiningTablesHandler
    : IRequestHandler<
        GetDiningTablesStatusQuery,
        List<GetDiningTableStatusDto>>
{
    private readonly IAppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetDiningTablesHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<GetDiningTableStatusDto>> Handle(
        GetDiningTablesStatusQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.DiningTables
            .Where(t => t.TableLayout.RestaurantId == _tenantContext.RequireRestaurantId())
            .AsQueryable();

        if (request.TableLayoutId.HasValue)
        {
            query = query.Where(t =>
                t.TableLayoutId == request.TableLayoutId.Value);
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