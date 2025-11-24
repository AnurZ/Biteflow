using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Querries.GetDiningTableList
{
    public sealed class GetDiningTableListQueryHandler : IRequestHandler<GetDiningTableListQuery, List<GetDiningTableListQueryDto>>
    {
        private readonly IAppDbContext _db;

        public GetDiningTableListQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<GetDiningTableListQueryDto>> Handle(GetDiningTableListQuery request, CancellationToken cancellationToken)
        {
            var query = _db.DiningTables.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SectionName))
                query = query.Where(t => t.SectionName == request.SectionName);

            if (request.Status.HasValue)
                query = query.Where(t => t.Status == request.Status.Value);

            if (request.MinimumSeats.HasValue)
                query = query.Where(t => t.NumberOfSeats >= request.MinimumSeats.Value);

            return await query
                .Select(t => new GetDiningTableListQueryDto
                {
                    Id = t.Id,
                    SectionName = t.SectionName,
                    Number = t.Number,
                    NumberOfSeats = t.NumberOfSeats,
                    TableType = t.TableType,
                    Status = t.Status,
                    IsActive = t.IsActive
                })
                .ToListAsync(cancellationToken);
        }
    }
}
