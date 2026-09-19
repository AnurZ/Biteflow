using Market.Domain.Entities.DiningTables;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;

namespace Market.Application.Modules.DiningTable.Commands.CreateDiningTable
{
    public sealed class CreateDiningTableCommandHandler
        : IRequestHandler<CreateDiningTableCommandDto, int>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public CreateDiningTableCommandHandler(
            IAppDbContext db,
            ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<int> Handle(
            CreateDiningTableCommandDto request,
            CancellationToken cancellationToken)
        {
            if (request.NumberOfSeats <= 0)
            {
                throw new ArgumentException(
                    "Number of seats must be greater than zero.");
            }

            var tenantId = _tenantContext.RequireTenantId();
            var restaurantId = _tenantContext.RequireRestaurantId();

            // Validate that the selected table layout belongs
            // to the current restaurant.
            var layoutExists = await _db.TableLayouts
                .WhereCurrentRestaurant(_tenantContext)
                .AnyAsync(
                    l => l.Id == request.TableLayoutId,
                    cancellationToken);

            if (!layoutExists)
            {
                throw new KeyNotFoundException(
                    $"TableLayout with ID {request.TableLayoutId} not found.");
            }

            // Only active tables occupy a table number.
            // Soft-deleted tables are allowed to use the same number again.
            var exists = await _db.DiningTables
                .AnyAsync(
                    t =>
                        t.TableLayoutId == request.TableLayoutId &&
                        t.Number == request.Number &&
                        !t.IsDeleted &&
                        t.TableLayout.RestaurantId == restaurantId,
                    cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"A table with Number '{request.Number}' already exists in this layout.");
            }

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
                Shape = request.Shape?.Trim() ?? string.Empty,
                Color = request.Color,

                TenantId = tenantId,
                IsDeleted = false
            };

            _db.DiningTables.Add(table);

            await _db.SaveChangesAsync(cancellationToken);

            return table.Id;
        }
    }
}