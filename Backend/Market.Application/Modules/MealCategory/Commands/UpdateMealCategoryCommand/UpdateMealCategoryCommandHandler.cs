using Market.Domain.Entities.Meal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Commands.UpdateMealCategoryCommand
{
    public sealed class UpdateMealCategoryCommandHandler(IAppDbContext db)
        : IRequestHandler<UpdateMealCategoryCommandDto>
    {
        public async Task Handle(UpdateMealCategoryCommandDto request, CancellationToken cancellationToken)
        {
            var mealCategory = await db.MealCategories
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (mealCategory is null)
                throw new KeyNotFoundException($"Meal category with ID {request.Id} not found.");

            var nameExists = await db.MealCategories
             .AnyAsync(m => m.Id != request.Id && m.Name == request.Name, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A meal category with the name '{request.Name.Trim()}' already exists.");

            mealCategory.Name = request.Name;
            mealCategory.Description = request.Description;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
