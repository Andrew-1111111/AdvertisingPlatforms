using System.Text;

namespace AdvertisingPlatforms.BusinessLogic.APlatforms.Interfaces
{
    public interface IApPlatformsRepository
    {
        bool IsEmpty{ get; }

        int Count{ get; }

        Task<bool> SetPlatformsAsync(byte[] data, Encoding encoding);

        IEnumerable<string> GetPlatforms(string location);

        IEnumerable<string> GetAllPlatforms();
    }
}