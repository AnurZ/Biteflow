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

            


            var table = new Domain.Entities.DiningTables.DiningTable
            {
                // Basic info
                SectionName = request.SectionName.Trim(),
                Number = request.Number,
                NumberOfSeats = request.NumberOfSeats,
                TableType = request.TableType,
                IsActive = true,
                Status = request.Status,

                // Layout/visual info
                TableLayoutId = request.TableLayoutId,
                X = request.X,
                Y = request.Y,
                Width = request.Width,
                Height = request.Height,
                Shape = request.Shape.Trim(),
                Color = request.Color
            };

            _db.DiningTables.Add(table);
            await _db.SaveChangesAsync(cancellationToken);

            return table.Id;
        }
    }
}
