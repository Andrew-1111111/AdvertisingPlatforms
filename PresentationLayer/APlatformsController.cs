using AdvertisingPlatforms.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text;

namespace AdvertisingPlatforms.Presentation
{
    [ApiController]
    [Route("Api/[controller]")]
    public class APlatformsController : ControllerBase
    {
        private static readonly Encoding _encoding = Encoding.UTF8;
        private static string _uploadDirectory = null!;
        private static string _localIndexFile = null!;
        private static ConcurrentDictionary<string, string[]> _concDictionary = null!;

        /// <summary>
        /// Конструктор, проверяет пути и инициализирует поля
        /// </summary>
        /// <param name="configuration">Конфигурация приложения (appsettings.json)</param>
        public APlatformsController(IConfiguration configuration)
        {
            // Устанавливаем пути к файлам
            _uploadDirectory ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.GetValue<string>("FileStorage")!);
            _localIndexFile ??= configuration.GetValue<string>("LocalIndexFile")!;

            // Если не существует, создаем директорию для загрузки файлов 'FileStorage'
            if (!Directory.Exists(_uploadDirectory))
                Directory.CreateDirectory(_uploadDirectory);

            // Инициализируем словарь
            _concDictionary ??= new ConcurrentDictionary<string, string[]>();
        }

        /// <summary>
        /// Отображение формы Index.html для загрузки файла в словарь
        /// </summary>
        /// <remarks>В ТЗ данный метод не заявлен, оставлен для удобства отладки!</remarks>
        /// <remarks>URL: /Api/APlatforms/*</remarks>
        /// <returns>ContentResult представляет собой обертку над HTML кодом (без нее разметка отображается как обычная строка)</returns>
        [HttpGet]
        public async Task<ContentResult> GetIndexAsync()
        {
            var contentType = "text/html";
            var fullPath = Path.Combine(_uploadDirectory, _localIndexFile);

            if (System.IO.File.Exists(fullPath) && new FileInfo(fullPath).Length > 0)
            {
                var html = await System.IO.File.ReadAllTextAsync(fullPath, _encoding);
                return base.Content(html.Trim(), contentType, _encoding);
            }
            else
            {
                return new ContentResult()
                {
                    Content = $"Файл '{fullPath}' не найден!",
                    ContentType = contentType,
                    StatusCode = 404
                };
            }
        }

        /// <summary>
        /// Возврат всех рекламных площадок (без обработки, as is)
        /// </summary>
        /// <remarks>В ТЗ данный метод не заявлен, оставлен для удобства отладки!</remarks>
        /// <remarks>URL: /Api/APlatforms/GetAllPlatforms</remarks>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        [HttpGet("GetAllPlatforms")]
        public IEnumerable<string> GetAllPlatforms()
        {
            if (_concDictionary != null && !_concDictionary.IsEmpty)
            {
                foreach (var kpVal in _concDictionary)
                    yield return $"{kpVal.Key}:{string.Join(",", kpVal.Value)}";
            }
            else
            {
                yield return "Данные не загружены. Загрузите файл с базой рекламных площадок через '/Api/APlatforms' и повторите запрос.";
            }
        }

        /// <summary>
        /// Метод загрузки рекламных площадок из файла (полностью перезаписывает всю хранимую информацию)
        /// </summary>
        /// <remarks>URL: /Api/APlatforms/Upload</remarks>
        /// <returns>ActionResult представляет различные коды состояния HTTP</returns>
        [HttpPost("Upload")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 402653184, ValueLengthLimit = 134217728)] // Общий лимит на загрузку 384 Мб, лимит каждого файла 128 Мб
        public async Task<IActionResult> PostAsync()
        {
            if (!Request.Form.Files.Any()) return NotFound(new { UploadStatus = "Файл не выбран. Необходимо выбрать файл." });
            if (_concDictionary == null) return NotFound(new { UploadStatus = "Файл не выбран. Необходимо выбрать файл." });

            var success = false;

            // Очищаем словарь
            if (!_concDictionary.IsEmpty) _concDictionary.Clear();

            // Перебираем полученные файлы
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    try
                    {
                        // Считываем файл из IFromFile в MemoryStream
                        await using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);

                        // Заносим данные из полученной строки (в выбранной кодировке) в ConcurrentDictionary
                        await Task.Run(()=> success = DictionaryHelper.SetDictionary(_encoding.GetString(ms.ToArray()), ref _concDictionary));
                    }
                    catch
                    {
                        // Тут можно подключить логирование
                    }
                }
            }

            if (success) return Accepted(new { UploadStatus = "Файл успешно загружен!" });
            else return BadRequest(new { UploadStatus = "Не удалось загрузить файл." });
        }

        /// <summary>
        /// Метод поиска списка рекламных площадок для заданной локации
        /// </summary>
        /// <remarks>URL: /Api/APlatforms/Search</remarks>
        /// <param name="location">Параметр запроса, вида: /ru/svrd</param>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        [HttpGet("Search")]
        public IEnumerable<string> Search([FromQuery] string location)
        {
            if (_concDictionary != null && !_concDictionary.IsEmpty)
            {
                if (!string.IsNullOrWhiteSpace(location) &&
                location.Contains('/') &&
                location[0] == '/')
                {
                    foreach (var ap in DictionaryHelper.GetPlatforms(location, _concDictionary))
                        yield return ap;
                }
                else
                {
                    yield return "Некорректный запрос. Исправьте и повторите его вновь.";
                }
            }
            else
            {
                yield return "Данные не загружены. Загрузите файл с базой рекламных площадок через '/Api/APlatforms' и повторите запрос.";
            }
        }
    }
}