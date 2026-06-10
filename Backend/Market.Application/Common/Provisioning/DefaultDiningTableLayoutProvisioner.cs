using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.TableLayout;

namespace Market.Application.Common.Provisioning;

public static class DefaultDiningTableLayoutProvisioner
{
    private const string MainFloorName = "Main Floor";

    private static readonly TableSpec[] MainFloorTables =
    [
        new(1, 2, 50, 80, 150, 80, "#e5e7eb", TableTypes.LowTable, TableStatus.Free),
        new(2, 2, 220, 80, 150, 80, "#dbeafe", TableTypes.LowTable, TableStatus.Free),
        new(3, 4, 390, 80, 150, 80, "#dbeafe", TableTypes.LowTable, TableStatus.Free),
        new(4, 4, 560, 80, 150, 80, "#dcfce7", TableTypes.LowTable, TableStatus.Serving),
        new(5, 4, 730, 80, 150, 80, "#e5e7eb", TableTypes.LowTable, TableStatus.Free),
        new(6, 2, 900, 80, 150, 80, "#fee2e2", TableTypes.LowTable, TableStatus.Paying),
        new(9, 6, 50, 200, 250, 100, "#dcfce7", TableTypes.Hightable, TableStatus.Serving),
        new(10, 6, 320, 200, 250, 100, "#dbeafe", TableTypes.Hightable, TableStatus.Seated),
        new(11, 6, 590, 200, 250, 100, "#e5e7eb", TableTypes.Hightable, TableStatus.Free),
        new(12, 6, 860, 200, 250, 100, "#e5e7eb", TableTypes.Hightable, TableStatus.Free)
    ];

    public static async Task EnsureMainFloorAsync(
        IAppDbContext db,
        Guid tenantId,
        Guid restaurantId,
        CancellationToken ct = default)
    {
        var layout = await db.TableLayouts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.RestaurantId == restaurantId &&
                x.Name == MainFloorName,
                ct);

        if (layout == null)
        {
            layout = new TableLayout
            {
                Name = MainFloorName,
                BackgroundColor = "#f5f5f5",
                FloorImageUrl = string.Empty,
                TenantId = tenantId,
                RestaurantId = restaurantId
            };

            db.TableLayouts.Add(layout);
            await db.SaveChangesAsync(ct);
        }

        var existingNumbers = await db.DiningTables
            .IgnoreQueryFilters()
            .Where(x =>
                x.TenantId == tenantId &&
                x.TableLayoutId == layout.Id)
            .Select(x => x.Number)
            .ToListAsync(ct);

        var existingNumberSet = existingNumbers.ToHashSet();
        var tables = MainFloorTables
            .Where(spec => !existingNumberSet.Contains(spec.Number))
            .Select(spec => new DiningTable
            {
                Number = spec.Number,
                NumberOfSeats = spec.Seats,
                IsActive = true,
                TableLayoutId = layout.Id,
                X = spec.X,
                Y = spec.Y,
                Width = spec.Width,
                Height = spec.Height,
                Shape = "rectangle",
                Color = spec.Color,
                TableType = spec.TableType,
                Status = spec.Status,
                TenantId = tenantId
            })
            .ToArray();

        if (tables.Length == 0)
        {
            return;
        }

        db.DiningTables.AddRange(tables);
        await db.SaveChangesAsync(ct);
    }

    private sealed record TableSpec(
        int Number,
        int Seats,
        int X,
        int Y,
        int Width,
        int Height,
        string Color,
        TableTypes TableType,
        TableStatus Status);
}
