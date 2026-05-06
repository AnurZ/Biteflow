using Market.Application.Modules.TableLayout.Queries.TableLayoutGetNameById;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class TableLayoutGetNameByIdHandler
    : IRequestHandler<TableLayoutGetNameByIdQuery, TableLayoutGetNameByIdDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public TableLayoutGetNameByIdHandler(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<TableLayoutGetNameByIdDto> Handle(
        TableLayoutGetNameByIdQuery request,
        CancellationToken cancellationToken)
    {
        var restaurantId = _tenantContext.RequireRestaurantId();
        var layout = await _context.TableLayouts
            .Where(x => x.Id == request.Id && x.RestaurantId == restaurantId)
            .Select(x => new TableLayoutGetNameByIdDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (layout is null)
            throw new KeyNotFoundException(
                $"Table layout with id '{request.Id}' was not found.");

        return layout;
    }
}
