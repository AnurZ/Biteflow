using Market.Application.Modules.DataImport.OrderImport;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderImportController : ControllerBase
    {
        private readonly IOrderImportService _service;

        public OrderImportController(IOrderImportService service)
        {
            _service = service;
        }

        [HttpPost("csv")]
        public async Task<IActionResult> ImportCsv(
        IFormFile file,
        CancellationToken ct)
        {
            var result = await _service.ImportFromCsv(file, ct);
            return Ok(new { imported = result });
        }

        [HttpPost("xlsx")]
        public async Task<IActionResult> ImportExcel(
        IFormFile file,
        CancellationToken ct)
        {
            var count = await _service.ImportFromExcel(file, ct);
            return Ok(new { imported = count });
        }
    }
}
