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

        public GetMealCategoriesHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<GetMealCategoriesDto>> Handle(GetMealCategoryQuery request, CancellationToken cancellationToken)
        {
            var categories = await _db.MealCategories
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
