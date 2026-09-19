using Market.Application.Abstractions;
using Market.Application.Modules.Meal.Queries.GetMealIngredients;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Market.Application.Modules.Meal.Queries.GetMealsByName
{
    public sealed class GetMealsByNameQueryHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
        : IRequestHandler<GetMealsByNameQuery, PageResult<GetMealsByNameDto>>
    {
        public async Task<PageResult<GetMealsByNameDto>> Handle(
            GetMealsByNameQuery request,
            CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var search = request.Name?.Trim().ToLower();

            var query = db.Meals
                .AsNoTracking()
                .Where(m => m.RestaurantId == restaurantId)
                .Select(m => new GetMealsByNameDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    BasePrice = m.BasePrice,
                    IsAvailable = m.IsAvailable,
                    IsFeatured = m.IsFeatured,
                    StockManaged = m.StockManaged,
                    ImageField = m.ImageField,
                    CategoryId = m.CategoryId,

                    Ingredients = m.Ingredients
                        .Select(mi => new MealIngredientQueryDto
                        {
                            InventoryItemId = mi.InventoryItemId,
                            InventoryItemName = mi.InventoryItem.Name,
                            Quantity = mi.Quantity,
                            UnitType = mi.UnitTypes.ToString(),
                        })
                        .ToList()
                })
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    m.Name.ToLower().StartsWith(search));
            }

            query = query.OrderBy(m => m.Name);

            return await PageResult<GetMealsByNameDto>
                .FromQueryableAsync(
                    query,
                    request.Paging,
                    cancellationToken);
        }
    }
}