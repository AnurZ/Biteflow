using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Queries.GetList
{
    public sealed class GetMealsQueryHandler(IAppDbContext db, ITenantContext tenantContext)
        : IRequestHandler<GetMealsQuery, PageResult<MealDto>>
    {
        public async Task<PageResult<MealDto>> Handle(GetMealsQuery request, CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var q = db.Meals
                .AsNoTracking()
                .WhereCurrentRestaurant(tenantContext)
                .Select(m => new MealDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    BasePrice = m.BasePrice,
                    IsAvailable = m.IsAvailable,
                    IsFeatured = m.IsFeatured,
                    ImageField = m.ImageField,
                    StockManaged = m.StockManaged,
                    IngredientsCount = m.Ingredients.Count,
                    CategoryId = m.CategoryId,
                    RestaurantId = restaurantId
                });

            // -------------------
            // SEARCH
            // -------------------
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim().ToLower();

                q = q.Where(x =>
                    x.Name.ToLower().Contains(s) ||
                    x.Description.ToLower().Contains(s));
            }

            if (request.CategoryId.HasValue && request.CategoryId > 0)
            {
                q = q.Where(x => x.CategoryId == request.CategoryId);
            }

            // -------------------
            // SORT
            // -------------------
            if (!string.IsNullOrWhiteSpace(request.Sort))
            {
                bool desc = request.Sort.StartsWith("-");
                string key = (desc ? request.Sort[1..] : request.Sort).ToLower();

                q = key switch
                {
                    "name" => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
                    "baseprice" => desc ? q.OrderByDescending(x => x.BasePrice) : q.OrderBy(x => x.BasePrice),
                    "isavailable" => desc ? q.OrderByDescending(x => x.IsAvailable) : q.OrderBy(x => x.IsAvailable),
                    "isfeatured" => desc ? q.OrderByDescending(x => x.IsFeatured) : q.OrderBy(x => x.IsFeatured),
                    "ingredientscount" => desc ? q.OrderByDescending(x => x.IngredientsCount) : q.OrderBy(x => x.IngredientsCount),
                    "category" => desc ? q.OrderByDescending(x => x.CategoryId) : q.OrderBy(x => x.CategoryId),
                    _ => q.OrderBy(x => x.Id)
                };
            }
            else
            {
                q = q.OrderBy(x => x.Id);
            }

            // -------------------
            // PAGINATION
            // -------------------
            return await PageResult<MealDto>.FromQueryableAsync(q, request.Paging, cancellationToken);
        }
    }
}
