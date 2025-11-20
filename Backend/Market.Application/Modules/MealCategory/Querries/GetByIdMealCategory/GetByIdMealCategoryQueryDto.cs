using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Querries.GetByIdMealCategory
{
    public sealed class GetByIdMealCategoryQuery : IRequest<GetMealCategoryByIdDto>
    {
        public int Id { get; set; }
    }

    public sealed class GetMealCategoryByIdDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
