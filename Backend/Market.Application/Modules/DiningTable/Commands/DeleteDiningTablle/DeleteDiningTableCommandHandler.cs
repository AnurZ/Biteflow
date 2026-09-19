using Market.Application.Modules.DiningTable.Commands.DeleteDiningTablle;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.DiningTable.Commands.DeleteDiningTable
{
    public sealed class DeleteDiningTableCommandHandler
        : IRequestHandler<DeleteDiningTableCommandDto>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public DeleteDiningTableCommandHandler(
            IAppDbContext db,
            ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task Handle(
            DeleteDiningTableCommandDto request,
            CancellationToken cancellationToken)
        {
            var restaurantId = _tenantContext.RequireRestaurantId();

            var table = await _db.DiningTables
                .Include(x => x.TableLayout)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.Id &&
                        !x.IsDeleted &&
                        x.TableLayout.RestaurantId == restaurantId,
                    cancellationToken);

            if (table == null)
            {
                throw new KeyNotFoundException(
                    $"Dining table with ID {request.Id} not found.");
            }

            table.IsDeleted = true;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}