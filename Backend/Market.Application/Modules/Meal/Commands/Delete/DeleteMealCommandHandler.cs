using Market.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Commands.Delete
{
    public sealed class DeleteMealCommandHandler(IAppDbContext db)
        : IRequestHandler<DeleteMealCommand>
    {
        public async Task Handle(DeleteMealCommand request, CancellationToken cancellationToken)
        {
            var meal = await db.Meals
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (meal is null)
                throw new KeyNotFoundException($"Meal with ID {request.Id} not found.");

            db.Meals.Remove(meal);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
