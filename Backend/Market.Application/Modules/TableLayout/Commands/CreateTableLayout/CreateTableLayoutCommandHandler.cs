using Market.Domain.Entities.TableLayout;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableLayout.Commands.CreateTableLayout
{
    public sealed class CreateTableLayoutCommandHandler : IRequestHandler<CreateTableLayoutCommandDto, int>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext tenantContext;

        public CreateTableLayoutCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            this.tenantContext=tenantContext;
        }

        public async Task<int> Handle(CreateTableLayoutCommandDto request, CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RestaurantId;

            if (restaurantId == null || restaurantId == Guid.Empty)
                throw new ValidationException("Restaurant context is missing.");

            var nameExists = await _db.TableLayouts
                .AnyAsync(m => m.Name.ToLower() == request.Name.Trim().ToLower()
                && m.RestaurantId == restaurantId, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{request.Name.Trim()}' already exists.");

            var layout = new Domain.Entities.TableLayout.TableLayout
            {
                Name = request.Name.Trim(),
                BackgroundColor = request.BackgroundColor,
                FloorImageUrl = request.FloorImageUrl,
                RestaurantId = restaurantId.Value,
            };

            _db.TableLayouts.Add(layout);
            await _db.SaveChangesAsync(cancellationToken);

            return layout.Id;
        }
    }
}
