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

        public UpdateDiningTableCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(UpdateDiningTableCommandDto request, CancellationToken cancellationToken)
        {
            // Find existing table
            var table = await _db.DiningTables.FindAsync(new object[] { request.Id }, cancellationToken);
            if (table == null)
                throw new KeyNotFoundException($"Dining table with ID {request.Id} not found.");

            // Validate inputs
            if (request.NumberOfSeats <= 0)
                throw new ArgumentException("Number of seats must be greater than zero.");

            // Apply updates - basic info
            table.SectionName = request.SectionName.Trim();
            table.Number = request.Number;
            table.NumberOfSeats = request.NumberOfSeats;
            table.IsActive = request.IsActive;

            // Apply layout/visual info
            table.TableLayoutId = request.TableLayoutId;
            table.X = request.X;
            table.Y = request.Y;
            table.Width = request.Width;
            table.Height = request.Height;
            table.Shape = request.Shape.Trim();
            table.Color = request.Color;

            // Apply status info
            table.TableType = request.TableType;
            table.Status = request.Status;
            table.LastUsedAt = request.LastUsedAt;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
