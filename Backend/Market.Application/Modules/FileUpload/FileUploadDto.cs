using Microsoft.AspNetCore.Http;

public class FileUploadDto
{
    public IFormFile File { get; set; } = null!;
}