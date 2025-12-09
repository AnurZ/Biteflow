using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Commands.DeleteDiningTablle
{
    public sealed class DeleteDiningTableCommandHandler(IAppDbContext _db) : IRequestHandler<DeleteDiningTableCommandDto>
    {

        public async Task Handle(DeleteDiningTableCommandDto request, CancellationToken cancellationToken)
        {

            var conn = (_db as DbContext)?.Database.GetDbConnection();
            Console.WriteLine($"EF is connected to: {conn?.ConnectionString}");

            // Load table with related collections to make sure cascade works
            var table = await _db.DiningTables.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);


            if (table == null)
                throw new KeyNotFoundException($"Dining table with ID {request.Id} not found.");

            _db.DiningTables.Remove(table);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
