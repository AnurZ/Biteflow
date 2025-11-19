using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Market.Infrastructure
{
    public class BlobStorageService
    {
        private readonly IConfiguration _configuration;
        private BlobContainerClient? _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            // ⚠ do NOT touch Azure here — constructor must be “safe” for Swagger
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            try
            {
                if (_containerClient == null)
                {
                    var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
                    var containerName = _configuration["AzureBlobStorage:ContainerName"];

                    if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
                        throw new Exception("Azure Blob configuration missing!");

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                    await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = _containerClient.GetBlobClient(fileName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blob upload failed: {ex}");
                throw;
            }
        }



        public string GetBlobUrl(string fileName)
        {
            if (_containerClient == null)
                throw new Exception("Container not initialized");

            var blobClient = _containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }



    }
}
