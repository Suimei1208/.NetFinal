using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Storage.Blob;
using System.Configuration;

namespace NetTechnology_Final.Services.IMG
{
    public class BlobService : IBlobService
    {
        private readonly CloudBlobClient _blobClient;
        private readonly string _containerName, _ConnectionString;

        public BlobService(CloudBlobClient blobClient, IConfiguration configuration)
        {
            _blobClient = blobClient;
            _containerName = configuration["AzureStorage:ContainerName"];
            _ConnectionString = configuration["AzureStorage:ConnectionString"];
        }

        public async Task<string> UploadBlobAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }
            using (MemoryStream fileUploadStream = new MemoryStream())
            {
                file.CopyTo(fileUploadStream);
                fileUploadStream.Position = 0;

                BlobContainerClient blobServiceClient = new BlobContainerClient(_ConnectionString, _containerName);

                string blobName = file.FileName;

                BlobClient blobClient = blobServiceClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(fileUploadStream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType },
                    Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
                });

                // Get the URL of the uploaded blob
                return blobClient.Uri.ToString();
            }
        }

        public async Task DeleteBlobAsync(string blobName)
        {
            BlobContainerClient blobServiceClient = new BlobContainerClient(_ConnectionString, _containerName);

            BlobClient blobClient = blobServiceClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteIfExistsAsync();
            }
        }

        // hàm cắt url ra
        public String TryGetBlobNameFromUrl(string url)
        {
            string blobName;
            var uri = new Uri(url);         
            return blobName = uri.Segments.Last().Trim('/');
        }

    }
}
