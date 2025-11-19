using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Market.Application.Modules.Meal.Queries.GetList
{
    public sealed class GetMealsQueryHandler(IAppDbContext _db) : IRequestHandler<GetMealsQuery, List<MealDto>>
    {
       
        public async Task<List<MealDto>> Handle(GetMealsQuery request, CancellationToken cancellationToken)
        {
            var meals = await _db.Meals
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

                })
                .ToListAsync(cancellationToken);

            return meals;
        }
    }
}
