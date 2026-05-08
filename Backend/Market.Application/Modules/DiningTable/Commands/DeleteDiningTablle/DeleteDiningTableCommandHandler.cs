using Market.Application.Modules.DiningTable.Commands.DeleteDiningTablle;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.DiningTable.Commands.DeleteDiningTable
{
    public sealed class DeleteDiningTableCommandHandler(
        IAppDbContext db,
        ITenantContext tenantContext
    ) : IRequestHandler<DeleteDiningTableCommandDto>
    {
        public async Task Handle(DeleteDiningTableCommandDto request, CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var table = await db.DiningTables
                .Include(x => x.TableLayout)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.TableLayout.RestaurantId == restaurantId,
                    cancellationToken);

            if (table == null)
                throw new KeyNotFoundException($"Dining table with ID {request.Id} not found.");

            db.DiningTables.Remove(table);

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
