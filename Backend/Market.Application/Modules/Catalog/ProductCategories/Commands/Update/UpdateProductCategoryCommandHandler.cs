namespace Market.Application.Modules.Catalog.ProductCategories.Commands.Update;

public sealed class UpdateProductCategoryCommandHandler(IAppDbContext ctx, ITenantContext tenantContext)
            : IRequestHandler<UpdateProductCategoryCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCategoryCommand request, CancellationToken ct)
    {
        var entity = await ctx.ProductCategories
            .WhereTenantOwned(tenantContext)
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MarketNotFoundException($"Kategorija (ID={request.Id}) nije pronađena.");

        // Check for duplicate name (case-insensitive, except for the same ID)
        var exists = await ctx.ProductCategories
            .WhereTenantOwned(tenantContext)
            .AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower(), ct);

        if (exists)
        {
            throw new MarketConflictException("Name already exists.");
        }

        entity.Name = request.Name.Trim();

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
