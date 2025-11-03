// Market.Application/Modules/TenantActivation/Queries/List/ListRequestsHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Market.Application.Abstractions;                       // IAppDbContext
using Market.Application.Common;                             // PageRequest, PageResult<T>
using Market.Application.Modules.TenantActivation.Queries.List;
using Market.Domain.Entities.Tenants;                        // ActivationDraftDto

namespace Market.Application.Modules.TenantActivation.Queries.List
{
    public sealed class ListRequestsHandler
        : IRequestHandler<ListRequestsQuery, PageResult<ActivationDraftDto>>
    {
        private readonly IAppDbContext _db;
        public ListRequestsHandler(IAppDbContext db) => _db = db;

        public async Task<PageResult<ActivationDraftDto>> Handle(ListRequestsQuery r, CancellationToken ct)
        {
            var q = _db.TenantActivationRequests.AsNoTracking();

            if (r.Status.HasValue)
                q = q.Where(x => x.Status == r.Status.Value);

            // sort: noviji prvo; fallback po Id
            var projected = q
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id)
                .Select(x => new ActivationDraftDto(
                    x.Id,
                    x.RestaurantName,
                    x.Domain,
                    x.OwnerFullName,
                    x.OwnerEmail,
                    x.OwnerPhone,
                    x.Address,
                    x.City,
                    x.State,
                    x.Status
                ));

            var paging = new PageRequest { Page = r.Page, PageSize = r.PageSize };

            return await PageResult<ActivationDraftDto>.FromQueryableAsync(projected, paging, ct);
        }
    }
}
