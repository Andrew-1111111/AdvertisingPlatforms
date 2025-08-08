namespace AdvertisingPlatforms.BusinessLogic
{
    public class QueryHelper
    {
        internal static bool Validation(string query)
        {
            if (!string.IsNullOrWhiteSpace(query) && query.Contains('/') && query[0] == '/')
                return true;
            return false;
        }
    }
}