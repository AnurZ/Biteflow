using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableReservation.Commands.DeleteTableReservation
{
    public sealed class DeleteTableReservationCommandHandler(IAppDbContext db)
        : IRequestHandler<DeleteTableReservationCommandDto>
    {
        public async Task Handle(DeleteTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var tr = await db.TableReservations.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (tr is null) 
                throw new KeyNotFoundException($"Table reservation with ID {request.Id} not found.");

            db.TableReservations.Remove(tr);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
