using Market.Infrastructure;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly BlobStorageService _blobService;

    public FileController(BlobStorageService blobService)
    {
        _blobService = blobService;
    }

    // 👇 This line is the key fix
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("No file uploaded");

        try
        {
            var url = await _blobService.UploadAsync(dto.File);
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
