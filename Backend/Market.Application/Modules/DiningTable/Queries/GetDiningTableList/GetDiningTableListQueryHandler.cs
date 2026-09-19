using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Market.Application.Modules.DiningTable.Queries.GetDiningTableList
{
    public sealed class GetDiningTableListQueryHandler
        : IRequestHandler<GetDiningTableListQuery, List<GetDiningTableListQueryDto>>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public GetDiningTableListQueryHandler(
            IAppDbContext db,
            ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<List<GetDiningTableListQueryDto>> Handle(
            GetDiningTableListQuery request,
            CancellationToken cancellationToken)
        {
            var query = _db.DiningTables
                .Include(t => t.TableLayout)
                .Where(t => t.TableLayout.RestaurantId == _tenantContext.RequireRestaurantId())
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.TableLayoutName))
            {
                query = query.Where(t =>
                    EF.Functions.Like(
                        t.TableLayout.Name,
                        request.TableLayoutName + "%"));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(t =>
                    t.Status == request.Status.Value);
            }

            if (request.MinimumSeats.HasValue)
            {
                query = query.Where(t =>
                    t.NumberOfSeats >= request.MinimumSeats.Value);
            }

            if (request.TableLayoutId.HasValue)
            {
                query = query.Where(t =>
                    t.TableLayoutId == request.TableLayoutId.Value);
            }

            if (request.Number.HasValue)
            {
                query = query.Where(t =>
                    t.Number == request.Number.Value);
            }

            if (request.Id.HasValue)
            {
                query = query.Where(t =>
                    t.Id == request.Id.Value);
            }

            return await query
                .Select(t => new GetDiningTableListQueryDto
                {
                    Id = t.Id,
                    TableLayoutId = t.TableLayoutId,
                    Number = t.Number,
                    NumberOfSeats = t.NumberOfSeats,
                    TableType = t.TableType,
                    Status = t.Status,
                    IsActive = t.IsActive,
                    X = t.X,
                    Y = t.Y,
                    Height = t.Height,
                    Width = t.Width,
                    Shape = t.Shape,
                    Color = t.Color,
                    LastUsedAt = t.LastUsedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}