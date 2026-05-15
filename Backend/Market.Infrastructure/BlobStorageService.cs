using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Market.Domain.Entities.BlobStorageSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Market.Infrastructure
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;
        private readonly BlobStorageSettings _settings;

        private static readonly HashSet<string> AllowedTypes =
            [
                "image/jpeg",
                "image/png",
                "image/webp"
            ];

        public BlobStorageService(
            IOptions<BlobStorageSettings> options,
            ILogger<BlobStorageService> logger)
        {
            _logger = logger;
            _settings = options.Value;

            if (string.IsNullOrWhiteSpace(_settings.ConnectionString) ||
                string.IsNullOrWhiteSpace(_settings.ContainerName))
            {
                throw new Exception("Azure Blob configuration missing.");
            }

            _containerClient = new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName);

            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<object> UploadAsync(IFormFile file)
        {
            if (!AllowedTypes.Contains(file.ContentType))
            {
                throw new Exception("Invalid file type.");
            }

            if (!IsValidImage(file.OpenReadStream(), file.ContentType))
            {
                throw new Exception("Invalid image content.");
            }

            var fileName = $"{Guid.NewGuid()}.jpg";

            var blobClient = _containerClient.GetBlobClient(fileName);

            try
            {
                using var inputStream = file.OpenReadStream();

                using var image = await Image.LoadAsync(inputStream);

                if (image.Width > 10000 || image.Height > 10000)
                {
                    throw new Exception("Image dimensions too large.");
                }

                image.Mutate(x =>
                {
                    x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(1920, 1080)
                    });
                });

                using var outputStream = new MemoryStream();

                await image.SaveAsJpegAsync(
                    outputStream,
                    new JpegEncoder
                    {
                        Quality = 75
                    });

                outputStream.Position = 0;

                await blobClient.UploadAsync(
                    outputStream,
                    new BlobHttpHeaders
                    {
                        ContentType = "image/jpeg"
                    });

                return new
                {
                    fileName,
                    url = CreateReadSasUrl(blobClient)
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

            return CreateReadSasUrl(blobClient);
        }

        private string CreateReadSasUrl(BlobClient blobClient)
        {
            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogError("Blob SAS generation failed because the client is not authenticated with a shared key credential.");
                throw new Exception("File access failed.");
            }

            var expiresOn = DateTimeOffset.UtcNow.AddMinutes(
                _settings.SasReadMinutes > 0 ? _settings.SasReadMinutes : 15);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = expiresOn
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
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
