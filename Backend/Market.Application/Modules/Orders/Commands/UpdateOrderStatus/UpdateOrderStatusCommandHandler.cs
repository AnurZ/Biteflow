using Market.Application.Abstractions;
using Market.Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Market.Application.Modules.Orders.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
    {
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
        {
            { OrderStatus.New, new[] { OrderStatus.Cooking, OrderStatus.Cancelled } },
            { OrderStatus.Cooking, new[] { OrderStatus.ReadyForPickup, OrderStatus.Cancelled } },
            { OrderStatus.ReadyForPickup, new[] { OrderStatus.Completed, OrderStatus.Cancelled } },
            { OrderStatus.Completed, Array.Empty<OrderStatus>() },
            { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
        };

        private readonly IAppDbContext _db;

        public UpdateOrderStatusCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with id {request.Id} not found.");
            }

            var current = order.Status;
            if (AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Length > 0)
            {
                if (!allowed.Contains(request.Status))
                {
                    throw new ValidationException($"Cannot change order status from {current} to {request.Status}.");
                }
            }
            else if (current == request.Status)
            {
                return;
            }
            else if (current is OrderStatus.Completed or OrderStatus.Cancelled)
            {
                throw new ValidationException("Completed or cancelled orders cannot change status.");
            }

            order.Status = request.Status;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
