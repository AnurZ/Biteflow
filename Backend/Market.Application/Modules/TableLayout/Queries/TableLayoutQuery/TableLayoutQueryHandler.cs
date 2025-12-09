using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Market.Domain.Entities.TableLayout;

namespace Market.Application.Modules.TableLayout.Querries.GetTableLayouts
{
    public sealed class GetTableLayoutsQueryHandler : IRequestHandler<GetTableLayoutsQuery, List<TableLayoutDto>>
    {
        private readonly IAppDbContext _db;

        public GetTableLayoutsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TableLayoutDto>> Handle(GetTableLayoutsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.TableLayouts
                .Include(l => l.Tables)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(l => l.Name.Contains(request.Name));

            return await query
                .Select(l => new TableLayoutDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    BackgroundColor = l.BackgroundColor,
                    FloorImageUrl = l.FloorImageUrl,
                    Tables = l.Tables.Select(t => new TableDto
                    {
                        Id = t.Id,
                        SectionName = t.SectionName,
                        Number = t.Number,
                        NumberOfSeats = t.NumberOfSeats,
                        X = t.X,
                        Y = t.Y,
                        TableSize = t.TableSize,
                        Shape = t.Shape,
                        Color = t.Color,
                        TableType = t.TableType,
                        Status = t.Status,
                        IsActive = t.IsActive
                    }).ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}
