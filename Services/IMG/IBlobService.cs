namespace NetTechnology_Final.Services.IMG
{
    public interface IBlobService
    {
        Task<string> UploadBlobAsync(IFormFile file);
        Task DeleteBlobAsync(string blobName);
        String TryGetBlobNameFromUrl(string url);
    }
}
