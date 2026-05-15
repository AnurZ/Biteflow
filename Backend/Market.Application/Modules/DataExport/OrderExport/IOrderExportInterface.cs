using System.Globalization;
using CsvHelper;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Market.Application.Modules.DataExport.OrderExport;

namespace Market.API.Services;

public interface IOrderExportService
{
    Task<byte[]> ExportOrdersCsv(OrderExportRequest request);

    Task<byte[]> ExportOrdersExcel(OrderExportRequest request);
}

public class OrderExportService : IOrderExportService
{
    private readonly IAppDbContext _context;

    public OrderExportService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportOrdersCsv(
        OrderExportRequest request)
    {
        var orders = await GetOrders(request);

        using var memoryStream = new MemoryStream();

        using var writer = new StreamWriter(memoryStream);

        using var csv = new CsvWriter(
            writer,
            CultureInfo.InvariantCulture);

        csv.WriteRecords(orders);

        await writer.FlushAsync();

        return memoryStream.ToArray();
    }

    public async Task<byte[]> ExportOrdersExcel(
        OrderExportRequest request)
    {
        var orders = await GetOrders(request);

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Orders");

        worksheet.Cell(1, 1).Value = "Order Id";
        worksheet.Cell(1, 2).Value = "Status";
        worksheet.Cell(1, 3).Value = "Total Price";
        worksheet.Cell(1, 4).Value = "Created At";
        worksheet.Cell(1, 5).Value = "Item Count";

        var headerRange = worksheet.Range(1, 1, 1, 6);

        headerRange.Style.Font.Bold = true;

        for (int i = 0; i < orders.Count; i++)
        {
            var row = i + 2;

            worksheet.Cell(row, 1).Value = orders[i].OrderId;

            worksheet.Cell(row, 2).Value =
                orders[i].Status.ToString();

            worksheet.Cell(row, 3).Value =
                orders[i].TotalPrice;

            worksheet.Cell(row, 4).Value =
                orders[i].CreatedAt;

            worksheet.Cell(row, 5).Value =
                orders[i].ItemCount;
        }

        worksheet.Column(3).Style.NumberFormat.Format =
            "$ #,##0.00";

        worksheet.Column(4).Style.DateFormat.Format =
            "dd.MM.yyyy HH:mm";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    private async Task<List<OrderExportDto>> GetOrders(
        OrderExportRequest request)
    {
        var query = _context.Orders
            .Include(x => x.Items)
            .AsQueryable();

            query = query.Where(x =>
                x.Status == request.Status);

        if (request.FromDate.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAtUtc >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAtUtc <= request.ToDate.Value);
        }

        var data = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new OrderExportDto
            {
                OrderId = x.Id,

                Status = x.Status,

                TotalPrice = x.Items.Sum(i =>
                    i.Quantity * i.UnitPrice),

                CreatedAt = x.CreatedAtUtc,

                ItemCount = x.Items.Count()
            })
            .ToListAsync();

        return data;
    }
}