using Market.Application.Modules.Meal.Queries.GetList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Querries.GetMealCategories
{
    public sealed class GetMealCategoryQuery : IRequest<List<GetMealCategoriesDto>>
    {
    }

    public sealed class GetMealCategoriesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
