namespace AdvertisingPlatforms.BusinessLogic.File.Interfaces
{
    public interface IFileRepository
    {
        string Name { get; set; }
        string Extension { get; set; }
        bool IsUploaded { get; set; }
        byte[] Data { get; set; }

        Task<bool> SetFileAsync(IFormFile file);

        Task ClearAsync();
    }
}