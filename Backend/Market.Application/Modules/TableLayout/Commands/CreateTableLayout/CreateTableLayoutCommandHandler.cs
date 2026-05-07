namespace Market.Application.Modules.TableLayout.Commands.CreateTableLayout
{
    public sealed class CreateTableLayoutCommandHandler
        : IRequestHandler<CreateTableLayoutCommandDto, int>
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
            var name = request.Name.Trim();

            // Tenant-scoped uniqueness
            var nameExists = await _db.TableLayouts
                .AnyAsync(x =>
                    x.TenantId == tenantId &&
                    x.Name.ToLower() == name.ToLower(),
                    cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{name}' already exists.");

            var layout = new Domain.Entities.TableLayout.TableLayout
            {
                Name = name,
                BackgroundColor = request.BackgroundColor,
                FloorImageUrl = request.FloorImageUrl,
                TenantId = tenantId
            };

            _db.TableLayouts.Add(layout);
            await _db.SaveChangesAsync(cancellationToken);

            return layout.Id;
        }
    }
}