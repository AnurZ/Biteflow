using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Commands.DeleteMealCategoryCommand
{
    public sealed class DeleteMealCategoryCommandHandler(IAppDbContext db)
        : IRequestHandler<DeleteMealCategoryCommandDto>
    {
        public async Task Handle(DeleteMealCategoryCommandDto request, CancellationToken cancellationToken)
        {
            var mealCategory = await db.MealCategories
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (mealCategory is null)
                throw new KeyNotFoundException($"Meal category with ID {request.Id} not found.");

            db.MealCategories.Remove(mealCategory);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
