using Market.Application.Modules.Meal.Queries.GetMealIngredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Querries.GetByIdMealCategory
{
    internal class GetByIdMealCategoryQueryHandler(IAppDbContext db)
        : IRequestHandler<GetByIdMealCategoryQuery, GetMealCategoryByIdDto>
    {
        public async Task<GetMealCategoryByIdDto> Handle(GetByIdMealCategoryQuery request, CancellationToken cancellationToken)
        {
            var mealCategory = await db.MealCategories
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (mealCategory == null)
                throw new KeyNotFoundException($"Meal category with ID {request.Id} not found.");

            return new GetMealCategoryByIdDto
            {
                Id = mealCategory.Id,
                Name = mealCategory.Name,
                Description = mealCategory.Description
            };
        }
    }
}
