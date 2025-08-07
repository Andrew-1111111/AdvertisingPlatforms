namespace AdvertisingPlatforms.BusinessLogic
{
    public class FileHelper
    {
        internal static bool FileValidation(IFormFile file)
        {
            if (file != null && file.Length > 0 && Path.GetExtension(file.FileName) == ".txt")
                return true;
            return false;
        }
    }
}