using Market.Domain.Entities.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Queries.GetById
{
    public sealed class GetTenantActivationRequestByIdHandler
       : IRequestHandler<GetTenantActivationByIdRequestQuery, ActivationDraftDto>
    {
        private readonly IAppDbContext _db;

        public GetTenantActivationRequestByIdHandler(IAppDbContext db) => _db = db;

        public async Task<ActivationDraftDto> Handle(GetTenantActivationByIdRequestQuery q, CancellationToken ct)
        {
            var dto = await _db.TenantActivationRequests
                .AsNoTracking()
                .Where(x => x.Id == q.Id)
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
                ))
                .SingleOrDefaultAsync(ct);

            if (dto is null)
                throw new MarketNotFoundException("Request not found");

            return dto;
        }
    }
}
