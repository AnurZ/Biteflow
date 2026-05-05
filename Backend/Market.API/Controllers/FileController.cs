using Market.Infrastructure;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.StaffMember)]
public class FileController : ControllerBase
{
    private readonly BlobStorageService _blobService;

    public FileController(BlobStorageService blobService)
    {
        _blobService = blobService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadDto dto)
    {
        if (dto.File is not { Length: > 0 } file)
            return BadRequest("No file uploaded");
        Console.WriteLine("FILE RECEIVED? => " + (file != null));

        try
        {
            var url = await _blobService.UploadAsync(file!);
            var fileName = Path.GetFileName(new Uri(url).LocalPath);
            return Ok(new { Url = url, FileName = fileName });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload failed: {ex}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{fileName}")]
    public IActionResult GetImageUrl(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("File name required");

        var url = _blobService.GetBlobUrl(fileName);
        return Ok(new { Url = url });
    }
}
