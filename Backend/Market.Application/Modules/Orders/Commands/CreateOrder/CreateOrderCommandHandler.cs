using Market.Application.Abstractions;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Orders;
using Market.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Market.Application.Modules.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
    {
        private readonly IAppDbContext _db;
        private readonly ITenantContext _tenantContext;
        private readonly IAppCurrentUser _currentUser;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            IAppDbContext db,
            ITenantContext tenantContext,
            IAppCurrentUser currentUser,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _db = db;
            _tenantContext = tenantContext;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantContext.RequireTenantId();
            var restaurantId = _tenantContext.RequireRestaurantId();

            if (request.Items == null || request.Items.Count == 0)
            {
                throw new ValidationException("Order must contain at least one item.");
            }

            ValidateItemShapes(request.Items);

            if (!request.DiningTableId.HasValue || request.DiningTableId.Value <= 0)
            {
                throw new ValidationException("DiningTableId is required.");
            }

            var table = await _db.DiningTables
                .AsNoTracking()
                .Include(t => t.TableLayout)
                .FirstOrDefaultAsync(
                    t => t.Id == request.DiningTableId.Value &&
                         t.IsActive &&
                         t.TableLayout.RestaurantId == restaurantId,
                    cancellationToken);

            if (table == null)
            {
                throw new KeyNotFoundException($"Dining table with ID {request.DiningTableId.Value} was not found or is inactive.");
            }

            var menuMealIds = request.Items
                .Where(item => !item.IsCustom)
                .Select(item => item.MealId!.Value)
                .Distinct()
                .ToArray();

            var mealsById = menuMealIds.Length == 0
                ? new Dictionary<int, MealSnapshot>()
                : await _db.Meals
                    .AsNoTracking()
                    .WhereCurrentRestaurant(_tenantContext)
                    .Where(meal => menuMealIds.Contains(meal.Id) && meal.IsAvailable)
                    .Select(meal => new MealSnapshot(meal.Id, meal.Name, meal.BasePrice))
                    .ToDictionaryAsync(meal => meal.Id, cancellationToken);

            var missingMealIds = menuMealIds
                .Where(mealId => !mealsById.ContainsKey(mealId))
                .ToArray();

            if (missingMealIds.Length > 0)
            {
                throw new ValidationException($"Meal(s) are unavailable or not found: {string.Join(", ", missingMealIds)}.");
            }

            var order = new Order
            {
                DiningTableId = table.Id,
                TableNumber = table.Number,
                Status = OrderStatus.New,
                Notes = request.Notes,
                TenantId = tenantId
            };

            var customOrderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                OrderItem orderItem;

                if (item.IsCustom)
                {
                    if (!CanCreateCustomItems())
                    {
                        throw new ValidationException("Custom order items are only allowed for waiter and admin roles.");
                    }

                    orderItem = new OrderItem
                    {
                        MealId = null,
                        Name = item.Name!.Trim(),
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice!.Value,
                        TenantId = tenantId
                    };

                    customOrderItems.Add(orderItem);
                }
                else
                {
                    var meal = mealsById[item.MealId!.Value];

                    orderItem = new OrderItem
                    {
                        MealId = meal.Id,
                        Name = meal.Name,
                        Quantity = item.Quantity,
                        UnitPrice = meal.BasePrice,
                        TenantId = tenantId
                    };
                }

                order.Items.Add(orderItem);
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var item in customOrderItems)
            {
                _logger.LogInformation(
                    "Custom order item accepted. TenantId={TenantId} UserId={UserId} OrderId={OrderId} OrderItemId={OrderItemId} DiningTableId={DiningTableId} ItemName={ItemName} UnitPrice={UnitPrice} Quantity={Quantity}",
                    tenantId,
                    _currentUser.UserId,
                    order.Id,
                    item.Id,
                    table.Id,
                    item.Name,
                    item.UnitPrice,
                    item.Quantity);
            }

            return order.Id;
        }

        private bool CanCreateCustomItems()
            => _currentUser.IsInRole(RoleNames.Waiter) || _currentUser.IsInRole(RoleNames.Admin);

        private static void ValidateItemShapes(IReadOnlyCollection<CreateOrderItemDto> items)
        {
            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                {
                    throw new ValidationException("Order item quantity must be greater than zero.");
                }

                if (item.IsCustom)
                {
                    if (item.MealId.HasValue)
                    {
                        throw new ValidationException("Custom order items cannot reference a meal.");
                    }

                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        throw new ValidationException("Custom order item name is required.");
                    }

                    if (item.Name.Length > 256)
                    {
                        throw new ValidationException("Custom order item name cannot exceed 256 characters.");
                    }

                    if (!item.UnitPrice.HasValue || item.UnitPrice.Value <= 0)
                    {
                        throw new ValidationException("Custom order item unit price must be greater than zero.");
                    }
                }
                else if (!item.MealId.HasValue || item.MealId.Value <= 0)
                {
                    throw new ValidationException("MealId is required for menu order items.");
                }
            }
        }

        private sealed record MealSnapshot(int Id, string Name, decimal BasePrice);
    }
}
