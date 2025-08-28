namespace AdvertisingPlatforms.BusinessLogic.Query
{
    /// <summary>
    /// Класс для валидации HTTP запросов
    /// </summary>
    internal class QueryHelper
    {
        /// <summary>
        /// Валидация строки HTTP GET запроса
        /// </summary>
        /// <param name="query">Строка GET запроса (пример: /ru/msk)</param>
        /// <returns>Bool флаг успешности прохождения валидации</returns>
        internal static bool LocationValidator(ref string query)
        {
            // Проверяем на пустоту
            if (!string.IsNullOrWhiteSpace(query)) // Не проверяем на наличие прямого слеша, потому что може быть запрос только с "location"
            {
                // Удаляем множественные пробелы
                do
                {
                    query = query.Replace("  ", " ");
                }
                while (query.Contains("  "));

                // Убираем лишние пробелы и символы переноса строк
                query = query.Trim();

                // Проверка на одиночный символ прямого слеша
                if (query == "/")
                {
                    query = string.Empty;
                    return false;
                }

                // Удаляем множественные слеши
                do
                {
                    query = query.Replace("//", "/");
                }
                while (query.Contains("//"));

                // Переводим в нижний регистр, убираем прямой слеш с конца строки
                query = query.TrimEnd('/').ToLower();

                // Если первым символом строки не является прямой слеш, добавляем его
                if (!query.StartsWith('/'))
                {
                    query = '/' + query;
                }

                return true;
            }

            query = string.Empty;
            return false;
        }
    }
}