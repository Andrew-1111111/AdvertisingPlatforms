namespace AdvertisingPlatforms.BusinessLogic.Query
{
    public class QueryHelper
    {
        /// <summary>
        /// Валидация строки HTTP GET запроса
        /// </summary>
        /// <param name="query">Строка GET запроса (пример: /ru/msk)</param>
        /// <returns>Bool флаг успешности прохождения валидации</returns>
        internal static bool LocationValidator(ref string query)
        {
            if (!string.IsNullOrWhiteSpace(query)) // Не проверяем на наличие прямого слеша, потому что може быть запрос только с "location"
            {
                // Переводим в нижний регистр, убираем прямой слеш с конца строки
                query = query.TrimEnd('/').ToLower();

                // Если первым символом строки не является прямой слеш, добавляем его
                if (!query.StartsWith('/'))
                {
                    query = '/' + query;
                }

                return true;
            }

            return false;
        }
    }
}