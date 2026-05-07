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
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadDto dto)
    {
        if (dto.File is not { Length: > 0 } file)
            return BadRequest("No file uploaded.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("Maximum file size is 5 MB.");

        try
        {
            var result = await _blobService.UploadAsync(file);

            return Ok(result);
        }
        catch
        {
            return StatusCode(500, new
            {
                error = "An unexpected error occurred."
            });
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
