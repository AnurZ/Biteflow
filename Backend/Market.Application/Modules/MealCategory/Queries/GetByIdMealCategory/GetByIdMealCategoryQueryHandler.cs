using Market.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.MealCategory.Queries.GetByIdMealCategory
{
    internal class GetByIdMealCategoryQueryHandler(
        IAppDbContext db,
        ITenantContext tenantContext)
        : IRequestHandler<GetByIdMealCategoryQuery, GetMealCategoryByIdDto>
    {
        public async Task<GetMealCategoryByIdDto> Handle(
            GetByIdMealCategoryQuery request,
            CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RequireRestaurantId();

            var mealCategory = await db.MealCategories
                .FirstOrDefaultAsync(
                    m => m.Id == request.Id &&
                         m.RestaurantId == restaurantId,
                    cancellationToken);

            if (mealCategory == null)
                throw new KeyNotFoundException(
                    $"Meal category with ID {request.Id} not found.");

            return new GetMealCategoryByIdDto
            {
                Id = mealCategory.Id,
                Name = mealCategory.Name,
                Description = mealCategory.Description
            };
        }
    }
}