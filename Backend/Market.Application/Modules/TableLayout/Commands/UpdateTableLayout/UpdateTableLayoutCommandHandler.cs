using Market.Domain.Entities.Meal;
using Market.Domain.Entities.TableLayout;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableLayout.Commands.UpdateTableLayout
{
    public sealed class UpdateTableLayoutCommandHandler : IRequestHandler<UpdateTableLayoutCommandDto>
    {
        private readonly IAppDbContext _db;

        public UpdateTableLayoutCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(UpdateTableLayoutCommandDto request, CancellationToken cancellationToken)
        {
            var layout = await _db.TableLayouts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (layout == null)
                throw new KeyNotFoundException($"TableLayout with ID {request.Id} not found.");

            var nameExists = await _db.TableLayouts
             .AnyAsync(m => m.Id != request.Id && m.Name == request.Name, cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{request.Name.Trim()}' already exists.");

            layout.Name = request.Name.Trim();
            layout.BackgroundColor = request.BackgroundColor;
            layout.FloorImageUrl = request.FloorImageUrl;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
