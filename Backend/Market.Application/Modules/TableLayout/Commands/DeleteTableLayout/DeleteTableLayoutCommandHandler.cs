using Market.Domain.Entities.TableLayout;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.TableLayout.Commands.DeleteTableLayout
{
    public sealed class DeleteTableLayoutCommandHandler : IRequestHandler<DeleteTableLayoutCommandDto>
    {
        private readonly IAppDbContext _db;

        public DeleteTableLayoutCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteTableLayoutCommandDto request, CancellationToken cancellationToken)
        {
            var layout = await _db.TableLayouts
                .Include(t => t.Tables)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (layout == null)
                throw new KeyNotFoundException($"TableLayout with ID {request.Id} not found.");

            if (layout.Tables != null && layout.Tables.Count > 0)
                throw new InvalidOperationException("Cannot delete a layout that has tables assigned.");

            _db.TableLayouts.Remove(layout);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
