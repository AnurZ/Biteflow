using Market.Domain.Entities.DiningTables;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.DiningTable.Commands.UpdateDiningTable
{
    public sealed class UpdateDiningTableCommandHandler
        : IRequestHandler<UpdateDiningTableCommandDto>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public UpdateDiningTableCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task Handle(UpdateDiningTableCommandDto request, CancellationToken cancellationToken)
        {
            var table = await _db.DiningTables
                .Include(x => x.TableLayout)
                .WhereTenantOwned(_tenantContext)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (table == null)
                throw new KeyNotFoundException($"Dining table with ID {request.Id} not found.");

            if (request.NumberOfSeats <= 0)
                throw new ArgumentException("Number of seats must be greater than zero.");

            // Validate layout belongs to tenant
            var layoutExists = await _db.TableLayouts
                .WhereTenantOwned(_tenantContext)
                .AnyAsync(l => l.Id == request.TableLayoutId, cancellationToken);

            if (!layoutExists)
                throw new KeyNotFoundException($"TableLayout with ID {request.TableLayoutId} not found.");

            // Duplicate number check (same layout)
            var numberExists = await _db.DiningTables
                .WhereTenantOwned(_tenantContext)
                .AnyAsync(t =>
                    t.TableLayoutId == request.TableLayoutId &&
                    t.Number == request.Number &&
                    t.Id != request.Id,
                    cancellationToken);

            if (numberExists)
                throw new ArgumentException(
                    $"A table with number {request.Number} already exists in this layout.");

            // Update
            table.Number = request.Number;
            table.NumberOfSeats = request.NumberOfSeats;
            table.IsActive = request.IsActive;

            table.TableLayoutId = request.TableLayoutId;
            table.X = request.X;
            table.Y = request.Y;
            table.Height = request.Height;
            table.Width = request.Width;

            table.Shape = request.Shape?.Trim() ?? string.Empty;
            table.Color = request.Color;

            table.TableType = request.TableType;
            table.Status = request.Status;
            table.LastUsedAt = request.LastUsedAt;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}