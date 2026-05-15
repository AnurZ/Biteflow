using ClosedXML.Excel;
using CsvHelper;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Orders;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public OrderImportService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportFromCsv(IFormFile file, CancellationToken ct)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var rows = csv.GetRecords<OrderImportRowDto>().ToList();

            return await Save(rows, ct);
        }

        public async Task<int> ImportFromExcel(IFormFile file, CancellationToken ct)
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var ws = workbook.Worksheet(1);

            var rows = new List<OrderImportRowDto>();

            var rowCount = ws.LastRowUsed().RowNumber();

            for (int i = 2; i <= rowCount; i++)
            {
                rows.Add(new OrderImportRowDto
                {
                    OrderId = ws.Cell(i, 1).GetValue<int?>(),
                    Status = ws.Cell(i, 2).GetString(),
                    DiningTableId = ws.Cell(i, 6).GetValue<int?>(),
                    TableNumber = ws.Cell(i, 7).GetValue<int?>(),
                    Notes = ws.Cell(i, 8).GetString()
                });
            }

            return await Save(rows, ct);
        }

        private async Task<int> Save(List<OrderImportRowDto> rows, CancellationToken ct)
        {
            var orders = new List<Order>();

            foreach (var x in rows)
            {
                if (!Enum.TryParse<OrderStatus>(x.Status ?? "", true, out var status))
                    status = OrderStatus.New;

                orders.Add(new Order
                {
                    Status = status,
                    DiningTableId = x.DiningTableId,
                    TableNumber = x.TableNumber,
                    Notes = x.Notes
                });
            }

            _context.Orders.AddRange(orders);

            await _context.SaveChangesAsync(ct);

            return orders.Count;
        }
    }
}
