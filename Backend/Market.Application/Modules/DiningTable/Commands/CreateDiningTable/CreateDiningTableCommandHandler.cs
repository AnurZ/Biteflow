using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Commands.CreateDiningTable
{
    public sealed class CreateDiningTableCommandHandler : IRequestHandler<CreateDiningTableCommandDto, int>
    {
        private readonly IAppDbContext _db;

        public CreateDiningTableCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<int> Handle(CreateDiningTableCommandDto request, CancellationToken cancellationToken)
        {
            if (request.NumberOfSeats <= 0)
                throw new ArgumentException("Number of seats must be greater than zero.");

            // Trim section name to normalize
            var sectionNameTrimmed = request.SectionName.Trim();

            // Check if a table with the same SectionName + Number already exists
            var exists = await _db.DiningTables
                .AnyAsync(t => t.SectionName == sectionNameTrimmed
                               && t.Number == request.Number
                               && !t.IsDeleted, // if soft delete is used
                               cancellationToken);

            if (exists)
                throw new InvalidOperationException($"A table with Section '{sectionNameTrimmed}' and Number '{request.Number}' already exists.");

            var table = new Domain.Entities.DiningTables.DiningTable
            {
                // Basic info
                SectionName = sectionNameTrimmed,
                Number = request.Number,
                NumberOfSeats = request.NumberOfSeats,
                TableType = request.TableType,
                IsActive = true,
                Status = request.Status,

                // Layout/visual info
                TableLayoutId = request.TableLayoutId,
                X = request.X,
                Y = request.Y,
                TableSize = request.TableSize,
                Shape = request.Shape.Trim(),
                Color = request.Color
            };

            _db.DiningTables.Add(table);
            await _db.SaveChangesAsync(cancellationToken);

            return table.Id;
        }

    }
}
