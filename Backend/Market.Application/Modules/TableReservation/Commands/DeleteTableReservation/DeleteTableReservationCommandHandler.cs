using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.TableReservation.Commands.DeleteTableReservation
{
    public sealed class DeleteTableReservationCommandHandler(
        IAppDbContext db,
        ITenantContext tenantContext
    ) : IRequestHandler<DeleteTableReservationCommandDto>
    {
        public async Task Handle(DeleteTableReservationCommandDto request, CancellationToken cancellationToken)
        {
            var reservation = await db.TableReservations
                .WhereTenantOwned(tenantContext)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (reservation is null)
                throw new KeyNotFoundException($"Table reservation with ID {request.Id} not found.");

            db.TableReservations.Remove(reservation);

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}