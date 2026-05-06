using Market.Domain.Entities.DiningTables;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Commands.UpdateDiningTable
{
    public sealed class UpdateDiningTableCommandHandler : IRequestHandler<UpdateDiningTableCommandDto>
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
            // Find existing table
            var table = await _db.DiningTables.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (table == null)
                throw new KeyNotFoundException($"Dining table with ID {request.Id} not found.");

            // Validate inputs
            if (request.NumberOfSeats <= 0)
                throw new ArgumentException("Number of seats must be greater than zero.");

            var restaurantId = _tenantContext.RequireRestaurantId();
            var layoutExists = await _db.TableLayouts
                .AnyAsync(l => l.Id == request.TableLayoutId && l.RestaurantId == restaurantId, cancellationToken);

            if (!layoutExists)
                throw new KeyNotFoundException($"TableLayout with ID {request.TableLayoutId} not found.");

            // Check for duplicate table number in the same layout
            bool numberExists = await _db.DiningTables
                .AnyAsync(t => t.TableLayoutId == request.TableLayoutId
                               && t.Number == request.Number
                               && t.Id != request.Id, cancellationToken);

            if (numberExists)
                throw new ArgumentException($"A table with number {request.Number} already exists in this layout.");

            // Update table properties
            table.Number = request.Number;
            table.NumberOfSeats = request.NumberOfSeats;
            table.IsActive = request.IsActive;

            table.TableLayoutId = request.TableLayoutId;
            table.X = request.X;
            table.Y = request.Y;
            table.Height = request.Height;
            table.Width = request.Width;
            table.Shape = request.Shape.Trim();
            table.Color = request.Color;

            table.TableType = request.TableType;
            table.Status = request.Status;
            table.LastUsedAt = request.LastUsedAt;

            await _db.SaveChangesAsync(cancellationToken);
        }

    }
}
