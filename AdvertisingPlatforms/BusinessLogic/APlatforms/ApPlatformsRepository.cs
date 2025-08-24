using AdvertisingPlatforms.BusinessLogic.APlatforms.Interfaces;
using System.Collections.Concurrent;
using System.Text;

namespace AdvertisingPlatforms.BusinessLogic.APlatforms
{
    /// <summary>
    /// Репозиторий, содержащий основную бизнес логику приложения
    /// </summary>
    public class ApPlatformsRepository : IApPlatformsRepository
    {
        /// <summary>
        /// Возвращает значение, указывающее, является ли объект пустым
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _concDictionary.IsEmpty;
            }
        }

        /// <summary>
        /// Получает число элементов, содержащихся в репозитории
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }

        private static readonly ConcurrentDictionary<string, List<string>> _concDictionary = new();
        private static int _count = 0; // Заводим отдельный счетчик колл-ва локаций, т.к. лишние вызовы к
                                       // ConcurrentDictionary.Count вызывают его полную внутреннюю блокировку

        /// <summary>
        /// Заполняет словарь с площадками
        /// </summary>
        /// <param name="data">Массив байт из строки в UTF-8 кодировке</param>
        /// <param name="encoding">UTF-8</param>
        /// <returns>Bool флаг успешности заполнения словаря</returns>
        public bool SetPlatforms(byte[] data, Encoding encoding)
        {
            // Заносим данные из полученной строки (в выбранной кодировке) в потоке из пула
            return SetDictionary(encoding.GetString(data));
        }

        /// <summary>
        /// Получает платформы по выбранной локации
        /// </summary>
        /// <param name="location">Локация (пример: /ru/msk)</param>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        public IEnumerable<string> GetPlatforms(string location)
        {
            if (_concDictionary.TryGetValue(location, out var list))
            {
                foreach (var line in list)
                    yield return line;
            }
        }

        /// <summary>
        /// Возвращает полный список локаций с платформами 
        /// </summary>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        public IEnumerable<string> GetAllPlatforms()
        {
            foreach (var kpVal in _concDictionary)
                yield return $"{kpVal.Key}:{string.Join(",", kpVal.Value)}";
        }

        /// <summary>
        /// Заполняет словарь из текста
        /// </summary>
        /// <param name="text">Текст, загруженный из пользовательского файла</param>
        /// <returns>Bool флаг успешности заполнения словаря</returns>
        private static bool SetDictionary(string text)
        {
            var success = false;

            // Создаем временный словарь
            var tempDict = new Dictionary<string, List<string>>();

            if (!string.IsNullOrWhiteSpace(text))
            {
                // Сбрасываем счетчик
                Interlocked.Exchange(ref _count, 0); 

                // Если файл имеет несколько строк
                if (text.Contains("\r\n") || text.Contains('\n'))
                {
                    var sourceText = text.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

                    foreach (var singleLine in sourceText)
                    {
                        if (string.IsNullOrWhiteSpace(singleLine) || !singleLine.Contains(':'))
                            continue;

                        var ap = "";
                        var locations = SplitLines(singleLine, ref ap);

                        if (!string.IsNullOrWhiteSpace(ap) && locations.Count > 0)
                        {
                            foreach (var location in locations)
                            {
                                if (!string.IsNullOrWhiteSpace(location))
                                {
                                    // Пытаемся добавить новое значение в словарь
                                    if (tempDict.TryAdd(location, [ap]))
                                    {
                                        Interlocked.Increment(ref _count);
                                        success = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else // Если файл имеет единственную строку, без символов переноса на новую строку
                {
                    var ap = "";
                    var locations = SplitLines(text, ref ap);

                    if (!string.IsNullOrWhiteSpace(ap) && locations.Count > 0)
                    {
                        foreach (var location in locations)
                        {
                            if (!string.IsNullOrWhiteSpace(location))
                            {
                                // Пытаемся добавить новое значение в словарь
                                if (tempDict.TryAdd(location, [ap])) 
                                {
                                    Interlocked.Increment(ref _count);
                                    success = true;
                                }
                            }
                        }
                    }
                }

                if (success)
                    AddNesting(tempDict);
            }

            return success;
        }

        /// <summary>
        /// Добавляет во временный словарь вложенные локации, заполняет основной ConcurrentDictionary
        /// </summary>
        /// <param name="tempDict">Временный словарь</param>
        private static void AddNesting(Dictionary<string, List<string>> tempDict)
        {
            // 1. Производим выборку и сохранение площадок во временный словарь 
            foreach (var selectedLocation in tempDict.Keys)
            {
                // Проходим по словарю 
                var values = tempDict
                    .Where(kpv => selectedLocation == kpv.Key || selectedLocation.StartsWith(kpv.Key + '/'))
                    .SelectMany(kpv => kpv.Value)
                    .Distinct()
                    .ToList();

                // Заменяем значения в словаре
                tempDict[selectedLocation] = values;
            }

            // 2. Заполняем и обновляем ConcurrentDictionary без удаления устаревших элементов
            foreach (var dictItem in tempDict)
                _concDictionary.AddOrUpdate(dictItem.Key, dictItem.Value, (key, oldValue) => dictItem.Value);

            // 3. Удаляем утаревшие элементы из ConcurrentDictionary
            foreach (var concKey in _concDictionary.Keys)
            {
                if (!tempDict.ContainsKey(concKey))
                    _concDictionary.TryRemove(concKey, out _);
            }
        }

        /// <summary>
        /// Разбивает строку на части по заданному алгоритму
        /// </summary>
        /// <param name="singleLine">Строка</param>
        /// <param name="value">Значение из словаря</param>
        /// <returns>Список строк (список ключей для словаря)</returns>
        private static List<string> SplitLines(string singleLine, ref string value)
        {
            try
            {
                var sourceLine = singleLine.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                value = sourceLine[0].Trim();
                var list = sourceLine[1].Split(',').ToList();
                return CleanList(list);
            }
            catch
            {
                // Игнорируем ошибки некорректного формата
            }

            return [];
        }

        /// <summary>
        /// Очищает пробелы в начале и конце каждой строки, переводит строку в нижний регистр
        /// </summary>
        /// <param name="list">Список, для форматирования и сохранения строк, изменения в нем сохраняются</param>
        /// <returns>Ссылка на список строк</returns>
        public static List<string> CleanList(List<string> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Trim().ToLower();
            }

            return list;
        }
    }
}