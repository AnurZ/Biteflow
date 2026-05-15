using Market.API.Services;
using Market.Application.Modules.DataExport.OrderExport;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderExportController : ControllerBase
{
    private readonly IOrderExportService _orderExportService;

    public OrderExportController(IOrderExportService orderExportService)
    {
        _orderExportService = orderExportService;
    }

    [HttpGet("export")]
    [Authorize(Policy = PolicyNames.RestaurantAdmin)]
    public async Task<IActionResult> Export(
        [FromQuery] OrderExportRequest request)
    {
        byte[] fileBytes;

        string contentType;
        string fileName;

        if (request.Format?.ToLower() == "xlsx")
        {
            fileBytes =
                await _orderExportService.ExportOrdersExcel(request);

            contentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            fileName = $"orders-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        }
        else
        {
            fileBytes =
                await _orderExportService.ExportOrdersCsv(request);

            contentType = "text/csv";

            fileName = $"orders-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        }

        return File(fileBytes, contentType, fileName);
    }
}