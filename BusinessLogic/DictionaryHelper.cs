using System.Collections.Concurrent;

namespace AdvertisingPlatforms.BusinessLogic
{
    internal static class DictionaryHelper
    {
        /// <summary>
        /// Осуществляет поиск в словаре по текстовой строке запроса локации
        /// </summary>
        /// <param name="location">Запрос локации</param>
        /// <param name="dict">Словарь</param>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        internal static IEnumerable<string> GetPlatforms(string location, ConcurrentDictionary<string, List<string>> dict)
        {
            var fullLocation = location.ToLower();

            foreach (var kpVal in dict)
            {
                foreach (var val in kpVal.Value)
                {
                    if (val == fullLocation)
                    {
                         yield return kpVal.Key;
                    }
                    else
                    {
                        if (location.StartsWith(val))
                        {
                            yield return kpVal.Key;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Заполняет словарь из текста
        /// </summary>
        /// <param name="text">Текст, загруженный из пользовательского файла</param>
        /// <param name="dict">Возвращаем временный словарь, ref для явного обозначения возврата по ссылке</param>
        /// <returns>Bool флаг успешности заполнения словаря</returns>
        internal static bool SetDictionary(string text, ref ConcurrentDictionary<string, List<string>> dict)
        {
            var success = false;

            if (!string.IsNullOrWhiteSpace(text))
            {
                // Если файл имеет несколько строк
                if (text.Contains("\r\n") || text.Contains('\n'))
                {
                    var sourceText = text.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

                    foreach (var singleLine in sourceText)
                    {
                        if (!string.IsNullOrWhiteSpace(singleLine) && singleLine.Contains(':'))
                        {
                            var key = "";
                            var values = SplitLines(singleLine, ref key);

                            if (!string.IsNullOrWhiteSpace(key) 
                                && values != null 
                                && values.Count > 0 
                                && !dict.TryAdd(key, values))
                                dict.AddOrUpdate(key, values, (key, oldValues) => values);  // Пытаемся добавить новое значение в dict, 
                                                                                            // если не получилось, обновляем значение этого ключа
                            success = true;
                        }
                    }
                }
                else // Если файл имеет единственную строку, без символов переноса на новую строку
                {
                    var key = "";
                    var values = SplitLines(text, ref key);

                    if (!string.IsNullOrWhiteSpace(key)
                                && values != null
                                && values.Count > 0
                                && !dict.TryAdd(key, values))
                        dict.AddOrUpdate(key, values, (key, oldValues) => values);  // Пытаемся добавить новое значение в dict, 
                                                                                    // если не получилось, обновляем значение этого ключа
                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// Разбивает строку на части по заданному алгоритму
        /// </summary>
        /// <param name="singleLine">Строка</param>
        /// <param name="key">Ключ из словаря</param>
        /// <returns>Список строк</returns>
        private static List<string> SplitLines(string singleLine, ref string key)
        {
            try
            {
                var sourceLine = singleLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
                key = sourceLine[0].Trim();
                var list = sourceLine[1].Split(',').ToList();
                CleanList(list);
                return list;
            }
            catch
            {
                // Тут можно подключить логирование или сделать дополнительную валидацию формата текста
            }

            return [];
        }

        /// <summary>
        /// Очищает пробелы в начале и конце каждой строки, переводит строку в нижний регистр
        /// </summary>
        /// <param name="arr">Список, из которого берутся и в который сохраняются строки</param>
        private static void CleanList(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].ToLower().Trim();
            }
        }

    }
}