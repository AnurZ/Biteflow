using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Commands.DeleteDiningTablle
{
    public sealed class DeleteDiningTableCommandHandler(IAppDbContext _db, ITenantContext _tenantContext) : IRequestHandler<DeleteDiningTableCommandDto>
    {

        public async Task Handle(DeleteDiningTableCommandDto request, CancellationToken cancellationToken)
        {
            var restaurantId = _tenantContext.IsSuperAdmin
                ? (Guid?)null
                : _tenantContext.RequireRestaurantId();

            var query = _db.DiningTables
                .Include(x => x.TableLayout)
                .WhereTenantOwned(_tenantContext);

            if (restaurantId.HasValue)
            {
                query = query.Where(x => x.TableLayout.RestaurantId == restaurantId.Value);
            }

            var table = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (table == null)
                throw new KeyNotFoundException($"Dining table with ID {request.Id} not found.");

            _db.DiningTables.Remove(table);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
