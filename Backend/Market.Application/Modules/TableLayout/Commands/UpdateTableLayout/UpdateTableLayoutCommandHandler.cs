using Market.Domain.Entities.Meal;
using Market.Domain.Entities.TableLayout;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableLayout.Commands.UpdateTableLayout
{
    public sealed class UpdateTableLayoutCommandHandler : IRequestHandler<UpdateTableLayoutCommandDto>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public UpdateTableLayoutCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task Handle(UpdateTableLayoutCommandDto request, CancellationToken cancellationToken)
        {
            var restaurantId = _tenantContext.IsSuperAdmin
                ? (Guid?)null
                : _tenantContext.RequireRestaurantId();

            var layoutQuery = _db.TableLayouts.WhereTenantOwned(_tenantContext);
            if (restaurantId.HasValue)
            {
                layoutQuery = layoutQuery.Where(x => x.RestaurantId == restaurantId.Value);
            }

            var layout = await layoutQuery
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (layout == null)
                throw new KeyNotFoundException($"TableLayout with ID {request.Id} not found.");

            var nameExists = await _db.TableLayouts
                .WhereTenantOwned(_tenantContext)
                .AnyAsync(m => m.Id != request.Id &&
                               (!restaurantId.HasValue || m.RestaurantId == restaurantId.Value) &&
                               m.Name == request.Name, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{request.Name.Trim()}' already exists.");

            layout.Name = request.Name.Trim();
            layout.BackgroundColor = request.BackgroundColor;
            layout.FloorImageUrl = request.FloorImageUrl;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
