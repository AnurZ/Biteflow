using ClosedXML.Excel;
using CsvHelper;
using Market.Application.Abstractions;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Market.Application.Modules.DataImport.OrderImport
{
    public interface IOrderImportService
    {
        Task<int> ImportFromCsv(IFormFile file, CancellationToken ct);

        Task<int> ImportFromExcel(IFormFile file, CancellationToken ct);
    }

    public class OrderImportService : IOrderImportService
    {
        private readonly IAppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public OrderImportService(
            IAppDbContext context,
            ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<int> ImportFromCsv(
            IFormFile file,
            CancellationToken ct)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            using var csv = new CsvReader(
                reader,
                CultureInfo.InvariantCulture);

            var rows = csv
                .GetRecords<OrderImportRowDto>()
                .ToList();

            return await Save(rows, ct);
        }

        public async Task<int> ImportFromExcel(
            IFormFile file,
            CancellationToken ct)
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var ws = workbook.Worksheet(1);

            var rows = new List<OrderImportRowDto>();

            var lastRow = ws.LastRowUsed();

            if (lastRow == null)
                return 0;

            var rowCount = lastRow.RowNumber();

            for (int i = 2; i <= rowCount; i++)
            {
                rows.Add(new OrderImportRowDto
                {
                    OrderId = ws.Cell(i, 1)
                        .GetValue<int?>(),

                    Status = ws.Cell(i, 2)
                        .GetString(),

                    CreatedAt = ws.Cell(i, 3)
                        .GetValue<DateTime?>(),

                    DiningTableId = ws.Cell(i, 4)
                        .GetValue<int?>(),

                    MealId = ws.Cell(i, 5)
                        .GetValue<int?>(),

                    Quantity = ws.Cell(i, 6)
                        .GetValue<int?>(),

                    UnitPrice = ws.Cell(i, 7)
                        .GetValue<decimal?>()
                });
            }

            return await Save(rows, ct);
        }

        private async Task<int> Save(
            List<OrderImportRowDto> rows,
            CancellationToken ct)
        {
            var restaurantId =
                _tenantContext.RequireRestaurantId();

            var tenantId =
                _tenantContext.RequireTenantId();

            if (rows.Count == 0)
                return 0;

            var orders = new List<Order>();

            // Same OrderId = same Order.
            // Each row becomes one OrderItem.
            foreach (var group in rows.GroupBy(x => x.OrderId))
            {
                var orderId = group.Key;

                if (!orderId.HasValue)
                {
                    throw new InvalidOperationException(
                        "OrderId is required for every imported row.");
                }

                var firstRow = group.First();

                // CreatedAt is required.
                if (!firstRow.CreatedAt.HasValue)
                {
                    throw new InvalidOperationException(
                        $"CreatedAt is required for order {orderId}.");
                }

                // Dining table is required.
                if (!firstRow.DiningTableId.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Dining table is required for order {orderId}.");
                }

                // Validate OrderStatus.
                if (!Enum.TryParse<OrderStatus>(
                        firstRow.Status ?? string.Empty,
                        true,
                        out var status))
                {
                    throw new InvalidOperationException(
                        $"Invalid order status '{firstRow.Status}' " +
                        $"for order {orderId}.");
                }

                // Verify that the dining table exists,
                // belongs to the current restaurant and is active.
                var table = await _context.DiningTables
                    .AsNoTracking()
                    .Include(t => t.TableLayout)
                    .FirstOrDefaultAsync(
                        t =>
                            t.Id == firstRow.DiningTableId.Value &&
                            t.IsActive &&
                            t.TableLayout.RestaurantId == restaurantId,
                        ct);

                if (table == null)
                {
                    throw new InvalidOperationException(
                        $"Dining table {firstRow.DiningTableId} " +
                        $"does not belong to the current restaurant " +
                        $"or is inactive.");
                }

                // Create the Order.
                //
                // The imported OrderId is NOT used as the
                // database primary key.
                //
                // EF will generate a new Order.Id.
                var order = new Order
                {
                    RestaurantId = restaurantId,
                    Status = status,
                    DiningTableId = table.Id,
                    TableNumber = table.Number,
                    TenantId = tenantId,
                    CreatedAtUtc = firstRow.CreatedAt.Value
                };

                // Every row in the group becomes an OrderItem.
                foreach (var row in group)
                {
                    // MealId is required.
                    if (!row.MealId.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"MealId is required for order {orderId}.");
                    }

                    // Quantity must be greater than zero.
                    if (!row.Quantity.HasValue ||
                        row.Quantity.Value <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Quantity must be greater than 0 " +
                            $"for order {orderId}.");
                    }

                    // UnitPrice cannot be negative.
                    if (!row.UnitPrice.HasValue ||
                        row.UnitPrice.Value < 0)
                    {
                        throw new InvalidOperationException(
                            $"UnitPrice must be greater than or equal to 0 " +
                            $"for order {orderId}.");
                    }

                    // Verify that the Meal exists and belongs
                    // to the current restaurant.
                    var meal = await _context.Meals
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            m =>
                                m.Id == row.MealId.Value &&
                                m.RestaurantId == restaurantId,
                            ct);

                    if (meal == null)
                    {
                        throw new InvalidOperationException(
                            $"Meal {row.MealId} does not exist " +
                            $"or does not belong to the current restaurant.");
                    }

                    // Add the item to the Order.
                    order.Items.Add(new OrderItem
                    {
                        MealId = meal.Id,
                        Name = meal.Name,
                        Quantity = row.Quantity.Value,
                        UnitPrice = row.UnitPrice.Value
                    });
                }

                orders.Add(order);
            }

            // Add Orders together with their OrderItems.
            _context.Orders.AddRange(orders);

            await _context.SaveChangesAsync(ct);

            // Return number of imported Orders,
            // not number of OrderItems.
            return orders.Count;
        }
    }
}