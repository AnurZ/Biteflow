using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;

namespace Market.Application.Modules.DiningTable.Commands.CreateDiningTable
{
    public sealed class CreateDiningTableCommandHandler : IRequestHandler<CreateDiningTableCommandDto, int>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public CreateDiningTableCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<int> Handle(CreateDiningTableCommandDto request, CancellationToken cancellationToken)
        {
            if (request.NumberOfSeats <= 0)
                throw new ArgumentException("Number of seats must be greater than zero.");

            var tenantId = _tenantContext.RequireTenantId();
            var restaurantId = _tenantContext.RequireRestaurantId();

            // 1. Validate layout belongs to the current restaurant
            var layoutExists = await _db.TableLayouts
                .AnyAsync(l =>
                    l.Id == request.TableLayoutId &&
                    l.TenantId == tenantId &&
                    l.RestaurantId == restaurantId,
                    cancellationToken);

            if (!layoutExists)
                throw new KeyNotFoundException($"TableLayout with ID {request.TableLayoutId} not found.");

            // 2. Prevent duplicate table number per layout + tenant safety
            var exists = await _db.DiningTables
                .AnyAsync(t =>
                    t.TableLayoutId == request.TableLayoutId &&
                    t.Number == request.Number &&
                    t.TableLayout.TenantId == tenantId &&
                    !t.IsDeleted,
                    cancellationToken);

            if (exists)
                throw new InvalidOperationException(
                    $"A table with Number '{request.Number}' already exists in this layout.");

            var table = new Domain.Entities.DiningTables.DiningTable
            {
                Number = request.Number,
                NumberOfSeats = request.NumberOfSeats,
                TableType = request.TableType,
                IsActive = true,
                Status = request.Status,

                TableLayoutId = request.TableLayoutId,
                X = request.X,
                Y = request.Y,
                Height = request.Height,
                Width = request.Width,
                Shape = request.Shape.Trim(),
                Color = request.Color,

                TenantId = tenantId
            };

            _db.DiningTables.Add(table);
            await _db.SaveChangesAsync(cancellationToken);

            return table.Id;
        }
    }
}
