using Market.Domain.Entities.TableLayout;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
            var tenantId = _tenantContext.RequireTenantId();
            var restaurantId = _tenantContext.RequireRestaurantId();

            var layout = await _db.TableLayouts
                .Where(x => x.TenantId == tenantId && x.RestaurantId == restaurantId)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (layout == null)
                throw new KeyNotFoundException($"TableLayout with ID {request.Id} not found.");

            var normalizedName = request.Name.Trim();

            var nameExists = await _db.TableLayouts
                .AnyAsync(x =>
                    x.TenantId == tenantId &&
                    x.RestaurantId == restaurantId &&
                    x.Id != request.Id &&
                    x.Name == normalizedName,
                    cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{normalizedName}' already exists.");

            layout.Name = normalizedName;
            layout.BackgroundColor = request.BackgroundColor;
            layout.FloorImageUrl = request.FloorImageUrl;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
