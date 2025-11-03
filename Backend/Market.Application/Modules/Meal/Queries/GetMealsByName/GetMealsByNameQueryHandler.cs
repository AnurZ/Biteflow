using Market.Application.Modules.Meal.Queries.GetMealIngredients;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Queries.GetMealsByName
{
    public sealed class GetMealsByNameQueryHandler(IAppDbContext db)
        : IRequestHandler<GetMealsByNameQuery, PageResult<GetMealsByNameDto>>
    {
        public async Task<PageResult<GetMealsByNameDto>> Handle(GetMealsByNameQuery request, CancellationToken cancellationToken)
        {
            var search = request.Name?.Trim().ToLower();

            // Base query
            var query = db.Meals
                .AsNoTracking()
                .Include(m => m.Ingredients)
                    .ThenInclude(mi => mi.InventoryItem)
                .Select(m => new GetMealsByNameDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    BasePrice = m.BasePrice,
                    IsAvailable = m.IsAvailable,
                    IsFeatured = m.IsFeatured,
                    ImageField = m.ImageField,
                    Ingredients = m.Ingredients.Select(mi => new MealIngredientQueryDto
                    {
                        InventoryItemId = mi.InventoryItemId,
                        InventoryItemName = mi.InventoryItem.Name,
                        Quantity = mi.Quantity,
                        UnitType = mi.UnitTypes.ToString(),
                    }).ToList()
                })
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    m.Name.ToLower().StartsWith(search.Trim().ToLower()));
            }

            

            // Return paged result
            return await PageResult<GetMealsByNameDto>.FromQueryableAsync(query, request.Paging, cancellationToken);
        }
    }
}
