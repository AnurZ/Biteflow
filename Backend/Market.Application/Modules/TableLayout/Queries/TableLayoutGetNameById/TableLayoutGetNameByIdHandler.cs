using Market.Application.Modules.TableLayout.Queries.TableLayoutGetNameById;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class TableLayoutGetNameByIdHandler
    : IRequestHandler<TableLayoutGetNameByIdQuery, TableLayoutGetNameByIdDto>
{
    private readonly IAppDbContext _context;

    public TableLayoutGetNameByIdHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TableLayoutGetNameByIdDto> Handle(
        TableLayoutGetNameByIdQuery request,
        CancellationToken cancellationToken)
    {
        var layout = await _context.TableLayouts
            .Where(x => x.Id == request.Id)
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
