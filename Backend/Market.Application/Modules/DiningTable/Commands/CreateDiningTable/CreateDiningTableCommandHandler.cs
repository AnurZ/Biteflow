using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Commands.CreateDiningTable
{
    public sealed class CreateDiningTableCommandHandler(IAppDbContext db)
        : IRequestHandler<CreateDiningTableCommandDto, int>
    {
        public async Task<int> Handle(CreateDiningTableCommandDto request, CancellationToken cancellationToken)
        {
            if (request.NumberOfSeats <= 0)
                throw new ArgumentException("Number of seats must be greater than zero.");

            var table = new Market.Domain.Entities.DiningTables.DiningTable
            {
                SectionName = request.SectionName.Trim(),
                Number = request.Number,
                NumberOfSeats = request.NumberOfSeats,
                TableType = request.TableType,
                IsActive = true,
                Status = Domain.Common.Enums.TableStatus.Free
            };

            db.DiningTables.Add(table);
            await db.SaveChangesAsync(cancellationToken);

            return table.Id;
        }
    }
}
