using Market.Application.Modules.Meal.Commands.Create;
using Market.Domain.Entities.MealCategory;
using MediatR;

namespace Market.Application.Modules.MealCategory.Commands.CreateMealCategoryCommand
{
    public sealed class CreateMealCategoryCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        : IRequestHandler<CreateMealCategoryCommand, int>
    {
        public async Task<int> Handle(CreateMealCategoryCommand request, CancellationToken cancellationToken)
        {
            var restaurantId = tenantContext.RestaurantId;

            if (restaurantId == null || restaurantId == Guid.Empty)
                throw new ValidationException("Restaurant context is missing.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("MealCategory name is required.");

            var nameExists = await db.MealCategories
                 .AnyAsync(m => m.Name.ToLower() == request.Name.Trim().ToLower()
                 && m.RestaurantId == restaurantId, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A category with the name '{request.Name.Trim()}' already exists.");


            var entity = new Domain.Entities.MealCategory.MealCategory
            {
                Name = request.Name,
                Description = request.Description,
                RestaurantId = restaurantId.Value,
            };

            db.MealCategories.Add(entity);

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
