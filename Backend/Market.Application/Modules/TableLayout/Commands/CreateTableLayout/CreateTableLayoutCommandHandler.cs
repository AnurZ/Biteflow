using Market.Domain.Entities.TableLayout;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableLayout.Commands.CreateTableLayout
{
    public sealed class CreateTableLayoutCommandHandler : IRequestHandler<CreateTableLayoutCommandDto, int>
    {
        private readonly IAppDbContext _db;

        public CreateTableLayoutCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<int> Handle(CreateTableLayoutCommandDto request, CancellationToken cancellationToken)
        {
            var nameExists = await _db.TableLayouts
                .AnyAsync(m => m.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

            if (nameExists)
                throw new ValidationException($"A table layout with the name '{request.Name.Trim()}' already exists.");

            var layout = new Domain.Entities.TableLayout.TableLayout
            {
                Name = request.Name.Trim(),
                BackgroundColor = request.BackgroundColor,
                FloorImageUrl = request.FloorImageUrl
            };

            _db.TableLayouts.Add(layout);
            await _db.SaveChangesAsync(cancellationToken);

            return layout.Id;
        }
    }
}
