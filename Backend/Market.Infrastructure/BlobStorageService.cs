using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Market.Infrastructure
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;

        private static readonly Dictionary<string, string> AllowedTypes = new()
        {
            { "image/jpeg", ".jpg" },
            { "image/png", ".png" },
            { "image/webp", ".webp" }
        };

        public BlobStorageService(
            IConfiguration configuration,
            ILogger<BlobStorageService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(containerName))
            {
                throw new Exception("Azure Blob configuration missing.");
            }

            _containerClient = new BlobContainerClient(connectionString, containerName);

            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<object> UploadAsync(IFormFile file)
        {
            if (!AllowedTypes.ContainsKey(file.ContentType))
            {
                throw new Exception("Invalid file type.");
            }

            if (!IsValidImage(file.OpenReadStream(), file.ContentType))
            {
                throw new Exception("Invalid image content.");
            }

            var extension = AllowedTypes[file.ContentType];

            var fileName = $"{Guid.NewGuid()}{extension}";

            var blobClient = _containerClient.GetBlobClient(fileName);

            try
            {
                using var stream = file.OpenReadStream();

                await blobClient.UploadAsync(
                    stream,
                    new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    });

                return new
                {
                    Url = blobClient.Uri.ToString(),
                    FileName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blob upload failed.");

                throw new Exception("File upload failed.");
            }
        }

        public string GetBlobUrl(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            return blobClient.Uri.ToString();
        }

        private bool IsValidImage(Stream stream, string contentType)
        {
            byte[] buffer = new byte[12];

            stream.Read(buffer, 0, buffer.Length);

            stream.Position = 0;

            return contentType switch
            {
                "image/jpeg" =>
                    buffer[0] == 0xFF &&
                    buffer[1] == 0xD8,

                "image/png" =>
                    buffer[0] == 0x89 &&
                    buffer[1] == 0x50 &&
                    buffer[2] == 0x4E &&
                    buffer[3] == 0x47,

                "image/webp" =>
                    buffer[8] == 0x57 &&
                    buffer[9] == 0x45 &&
                    buffer[10] == 0x42 &&
                    buffer[11] == 0x50,

                _ => false
            };
        }
    }
}