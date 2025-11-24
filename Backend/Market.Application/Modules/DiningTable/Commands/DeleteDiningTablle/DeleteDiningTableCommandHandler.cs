using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Commands.DeleteDiningTablle
{
   public sealed class DeleteDiningTableCommandHandler : IRequestHandler<DeleteDiningTableCommandDto>
{
    private readonly IAppDbContext _db;

    public DeleteDiningTableCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteDiningTableCommandDto request, CancellationToken cancellationToken)
    {
        var exists = await _db.DiningTables.FirstOrDefaultAsync(dt => dt.Id == request.Id, cancellationToken);
        if (exists == null)
            throw new KeyNotFoundException($"Dining table with ID {request.Id} not found.");

        _db.DiningTables.Remove(exists);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
}
