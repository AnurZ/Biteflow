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
            var restaurantId = _tenantContext.RequireRestaurantId();
            var layout = await _db.TableLayouts
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.RestaurantId == restaurantId, cancellationToken);

            if (layout == null)
                throw new KeyNotFoundException($"TableLayout with ID {request.Id} not found.");

            var nameExists = await _db.TableLayouts
             .AnyAsync(m => m.Id != request.Id && m.RestaurantId == restaurantId && m.Name == request.Name, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{request.Name.Trim()}' already exists.");

            layout.Name = request.Name.Trim();
            layout.BackgroundColor = request.BackgroundColor;
            layout.FloorImageUrl = request.FloorImageUrl;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
