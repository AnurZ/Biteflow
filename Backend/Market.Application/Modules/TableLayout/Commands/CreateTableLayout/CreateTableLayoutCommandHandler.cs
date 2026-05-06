namespace Market.Application.Modules.TableLayout.Commands.CreateTableLayout
{
    public sealed class CreateTableLayoutCommandHandler : IRequestHandler<CreateTableLayoutCommandDto, int>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public CreateTableLayoutCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<int> Handle(CreateTableLayoutCommandDto request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantContext.RequireTenantId();
            var restaurantId = _tenantContext.RequireRestaurantId();
            var name = request.Name.Trim();

            var nameExists = await _db.TableLayouts
                .AnyAsync(m => m.Name.ToLower() == name.ToLower()
                               && m.RestaurantId == restaurantId, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{name}' already exists.");

            var layout = new Domain.Entities.TableLayout.TableLayout
            {
                Name = name,
                BackgroundColor = request.BackgroundColor,
                FloorImageUrl = request.FloorImageUrl,
                RestaurantId = restaurantId,
                TenantId = tenantId
            };

            _db.TableLayouts.Add(layout);
            await _db.SaveChangesAsync(cancellationToken);

            return layout.Id;
        }
    }
}
