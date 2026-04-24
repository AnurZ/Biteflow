using Market.Application.Modules.Meal.Queries.GetList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Querries.GetMealCategories
{
    public sealed class GetMealCategoriesHandler : IRequestHandler<GetMealCategoryQuery, List<GetMealCategoriesDto>>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public GetMealCategoriesHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<List<GetMealCategoriesDto>> Handle(GetMealCategoryQuery request, CancellationToken cancellationToken)
        {
            var categories = await _db.MealCategories
                .Where(x=> x.RestaurantId == _tenantContext.RestaurantId)
                .Select(c => new GetMealCategoriesDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync(cancellationToken);

            return categories;
        }
    }

}
