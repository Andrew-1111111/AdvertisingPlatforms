using AdvertisingPlatforms.BusinessLogic.APlatforms.Interfaces;
using AdvertisingPlatforms.BusinessLogic.File.Interfaces;
using AdvertisingPlatforms.BusinessLogic.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AdvertisingPlatforms.PresentationLayer
{
    [ApiController]
    [Route("Api/[controller]")]
    public class APlatformsController : ControllerBase
    {
        private static string _uploadDirectory = null!;
        private static string _localIndexFile = null!;
        private static readonly Encoding _encoding = Encoding.UTF8;
        private readonly IApPlatformsRepository _apRepository = null!;
        private readonly ILogger<APlatformsController> _logger;

        /// <summary>
        /// Конструктор, проверяет пути и инициализирует поля
        /// </summary>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <param name="logger">ILogger сохраняет ошибки в контроллере</param>
        public APlatformsController(IConfiguration configuration, ILogger<APlatformsController> logger, IApPlatformsRepository apRepository)
        {
            // Устанавливаем пути к файлам
            _uploadDirectory ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.GetValue<string>("FileStorage")!);
            _localIndexFile ??= configuration.GetValue<string>("LocalIndexFile")!;

            // Инициализируем logger (для каждого экземпляра класса)
            _logger = logger;

            // Инициализируем ApPlatrorm репозиторий
            _apRepository = apRepository;

            // Если не существует, создаем директорию для загрузки файлов 'FileStorage'
            if (!Directory.Exists(_uploadDirectory))
                Directory.CreateDirectory(_uploadDirectory);
        }

        /// <summary>
        /// Отображение формы Index.html для загрузки файла в словарь
        /// </summary>
        /// <remarks>В ТЗ данный метод не заявлен, оставлен для удобства отладки!</remarks>
        /// <remarks>URL: /Api/APlatforms/*</remarks>
        /// <returns>ContentResult представляет собой обертку над HTML кодом (без нее разметка отображается как обычная строка)</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ContentResult> GetIndexAsync()
        {
            const string contentType = "text/html";
            var fullPath = Path.Combine(_uploadDirectory, _localIndexFile);

            if (System.IO.File.Exists(fullPath))
            {
                if (new FileInfo(fullPath).Length > 0)
                {
                    var html = await System.IO.File.ReadAllTextAsync(fullPath, _encoding);
                    return base.Content(html.Trim(), contentType, _encoding);
                }
                throw new IOException($"Файл '{fullPath}' пуст!");
            }
            throw new FileNotFoundException($"Файл '{fullPath}' не найден!");
        }

        /// <summary>
        /// Метод загрузки рекламных площадок из файла (полностью перезаписывает всю хранимую информацию)
        /// </summary>
        /// <remarks>URL: /Api/APlatforms/Upload</remarks>
        /// <returns>ActionResult представляет различные коды состояния HTTP</returns>
        [HttpPost("UploadFile")]
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 402653184, ValueLengthLimit = 134217728)] // Общий лимит на загрузку 384 Мб, лимит каждого файла 128 Мб
        public async Task<IActionResult> UploadFileAsync(IFileRepository fileRepository)
        {
            if (!Request.Form.Files.Any()) return NotFound(new { UploadStatus = "Файл не выбран. Необходимо выбрать файл." });

            var success = false;

            // Перебираем полученные файлы
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    try
                    {
                        // Устанавливает и проверяет файл в репозитории
                        if (await fileRepository.SetFileAsync(file))
                        {
                            // Заполняем словарь с рекламными площадками
                            success = await _apRepository.SetPlatformsAsync(fileRepository.Data, _encoding);

                            // Очищаем свойства файлового репозитория
                            await fileRepository.ClearAsync();
                        }
                    }
                    catch (Exception ex) // Даже при реализованной глобальной обработке ошибок не убираем этот блок, на случай того, что следующий файл будет валидным
                    {
                        _logger.LogError("{ex.ToString()}", ex.ToString());
                    }
                }
            }

            if (success) return Accepted(new { UploadStatus = $"База рекламных площадок успешно обновлена. Общее число локаций: {_apRepository.Count}." });
            return BadRequest(new { UploadStatus = "Не удалось загрузить файл." });
        }

        /// <summary>
        /// Возврат всех рекламных площадок (без обработки, as is)
        /// </summary>
        /// <remarks>В ТЗ данный метод не заявлен, оставлен для удобства отладки!</remarks>
        /// <remarks>URL: /Api/APlatforms/GetAllPlatforms</remarks>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        [HttpGet("GetAllPlatforms")]
        [AllowAnonymous]
        public IEnumerable<string> GetAllPlatforms()
        {
            if (!_apRepository.IsEmpty)
            {
                foreach (var ap in _apRepository.GetAllPlatforms())
                    yield return ap;
            }
            else
            {
                throw new FileNotFoundException(
                    $"Загрузите файл с базой рекламных площадок через '/Api/{ControllerContext.ActionDescriptor.ControllerName}' и повторите запрос.");
            }
        }

        /// <summary>
        /// Метод поиска списка рекламных площадок для заданной локации
        /// </summary>
        /// <remarks>URL: /Api/APlatforms/Search</remarks>
        /// <param name="location">Параметр запроса, вида: /ru/svrd</param>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        [HttpGet("SearchPlatforms")]
        [AllowAnonymous]
        public IEnumerable<string> SearchPlatforms([FromQuery] string location)
        {
            if (QueryHelper.LocationValidator(ref location)) // Проводим валидацию строки запроса
            {
                if (!_apRepository.IsEmpty)
                {
                    foreach (var line in _apRepository.GetPlatforms(location))
                    {
                        yield return line;
                    }
                }
                else
                {
                    throw new FileNotFoundException(
                        $"Загрузите файл с базой рекламных площадок через '/Api/{ControllerContext.ActionDescriptor.ControllerName}' и повторите запрос.");
                }
            }
            else throw new ArgumentException("Ошибка валидации запроса. Исправьте и повторите его вновь.");
        }
    }
}