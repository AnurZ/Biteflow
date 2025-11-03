using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Queries.GetMealsByName
{
    public sealed class GetMealsByNameQuery: BasePagedQuery<GetMealsByNameDto>
    {
        public string Name { get; init; }
    }
}
