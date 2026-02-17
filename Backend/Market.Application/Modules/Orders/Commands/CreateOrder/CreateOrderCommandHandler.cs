using Market.Application.Abstractions;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Orders;
using Market.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Market.Application.Modules.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;

        public CreateOrderCommandHandler(IAppDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new ValidationException("Order must contain at least one item.");
            }

            int? tableNumber = request.TableNumber;
            int? diningTableId = request.DiningTableId;

            if (request.DiningTableId.HasValue)
            {
                var table = await _db.DiningTables
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == request.DiningTableId.Value, cancellationToken);

                if (table == null)
                {
                    // If the provided table does not exist, fall back to loose table number only.
                    diningTableId = null;
                }
                else
                {
                    tableNumber ??= table.Number;
                    diningTableId = table.Id;
                }
            }

            if (!tableNumber.HasValue)
            {
                throw new ValidationException("Order must include a table number when no dining table is found.");
            }

            var order = new Order
            {
                DiningTableId = diningTableId,
                TableNumber = tableNumber,
                Status = OrderStatus.New,
                Notes = request.Notes,
                TenantId = _tenantContext.TenantId ?? SeedConstants.DefaultTenantId
            };

            foreach (var item in request.Items)
            {
                order.Items.Add(new OrderItem
                {
                    MealId = item.MealId,
                    Name = item.Name.Trim(),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TenantId = _tenantContext.TenantId ?? SeedConstants.DefaultTenantId
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
