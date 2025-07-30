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
        internal static IEnumerable<string> GetPlatforms(string location, ConcurrentDictionary<string, string[]> dict)
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
        /// <param name="dict">Словарь</param>
        /// <returns>Bool флаг успешности заполнения словаря</returns>
        internal static bool SetDictionary(string text, ref ConcurrentDictionary<string, string[]> dict)
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
                            string[] values = null!;
                            SplitLines(singleLine, ref key, ref values);

                            if (!string.IsNullOrWhiteSpace(key) 
                                && values != null 
                                && values.Length > 0 
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
                    string[] values = null!;
                    SplitLines(text, ref key, ref values);

                    if (!string.IsNullOrWhiteSpace(key)
                                && values != null
                                && values.Length > 0
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
        /// <param name="values">Массив занчений из словаря</param>
        private static void SplitLines(string singleLine, ref string key, ref string[] values)
        {
            try
            {
                var sourceLine = singleLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
                key = sourceLine[0].Trim();
                values = sourceLine[1].Split(',');
                CleanArray(ref values);
            }
            catch
            {
                // Тут можно подключить логирование или сделать дополнительную валидацию формата текста
            }
        }

        /// <summary>
        /// Очищает пробелы в начале и конце каждой строки, переводит строку в нижний регистр
        /// </summary>
        /// <param name="arr">Массив, из которого берутся строки</param>
        private static void CleanArray(ref string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = arr[i].ToLower().Trim();
            }
        }

    }
}