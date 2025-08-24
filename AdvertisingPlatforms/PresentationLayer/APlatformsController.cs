using AdvertisingPlatforms.BusinessLogic.APlatforms.Interfaces;
using AdvertisingPlatforms.BusinessLogic.File.Interfaces;
using AdvertisingPlatforms.BusinessLogic.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AdvertisingPlatforms.PresentationLayer
{
    /// <summary>
    /// Контроллер страницы рекламных площадок
    /// </summary>
    [ApiController]
    [Route("Api/{culture:culture}/[controller]")]
    public class APlatformsController : ControllerBase
    {
        private static string _uploadDirectory = null!;
        private static string _localIndexFile = null!;
        private static readonly Encoding _encoding = Encoding.UTF8;
        private readonly ILogger<APlatformsController> _logger;
        private readonly IApPlatformsRepository _apRepository = null!;
        private readonly IStringLocalizer<APlatformsController> _localizer;

        /// <summary>
        /// Конструктор, инициализирует поля и проверяет пути
        /// </summary>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <param name="logger">ILogger сохраняет ошибки в контроллере</param>
        /// <param name="apRepository">IApPlatformsRepository представляет собой интерфейс для работы с файловым репозиторием</param>
        /// <param name="localizer">IStringLocalizer представляет собой интерфейс для работы с локализацией приложения</param>
        public APlatformsController(IConfiguration configuration, ILogger<APlatformsController> logger, 
            IApPlatformsRepository apRepository, IStringLocalizer<APlatformsController> localizer)
        {
            // Устанавливаем пути к файлам
            _uploadDirectory ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.GetValue<string>("FileStorage")!);
            _localIndexFile ??= configuration.GetValue<string>("LocalIndexFile")!;

            // Инициализируем logger (для каждого экземпляра класса)
            _logger = logger;

            // Инициализируем ApPlatforms репозиторий
            _apRepository = apRepository;

            // Инициализируем локализацию
            _localizer = localizer;

            // Если не существует, создаем директорию для загрузки файлов 'FileStorage'
            if (!Directory.Exists(_uploadDirectory))
                Directory.CreateDirectory(_uploadDirectory);
        }

        /// <summary>
        /// Отображение формы Index.html для загрузки файла в словарь
        /// </summary>
        /// <remarks>URL: /Api/{Culture}/APlatforms/*</remarks>
        /// <returns>ContentResult представляет собой обертку над HTML кодом (без нее разметка отображается как обычная строка)</returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="500">Внутренняя ошибка сервера, в случае возникновения исключения</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                throw new IOException(_localizer["IOException_P1"] + $" {fullPath} " + _localizer["IOException_P2"]);
            }
            throw new FileNotFoundException(_localizer["FileNotFoundException1_P1"] + $" {fullPath} " + _localizer["FileNotFoundException1_P2"]);
        }

        /// <summary>
        /// Метод загрузки рекламных площадок из файла (полностью перезаписывает всю хранимую информацию)
        /// </summary>
        /// <remarks>URL: /Api/{Culture}/APlatforms/UploadFile
        /// 
        /// POST /UploadFile
        /// {
        ///   "files" : { 
        ///      "someFile" : "TestCaseFile.txt"
        ///      }
        /// }
        /// 
        /// </remarks>
        /// <returns>ActionResult представляет различные коды состояния HTTP</returns>
        /// <response code="202">Успешное выполнение</response>
        /// <response code="400">Файл загружен, но в процессе его обработки возникли ошибки валидации или исключения</response>
        /// <response code="404">Файл не загружен пользователем</response>
        /// <response code="500">Внутренняя ошибка сервера, в случае возникновения исключения</response>
        [HttpPost("UploadFile")]
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 402653184, ValueLengthLimit = 134217728)] // Общий лимит на загрузку 384 Мб, лимит каждого файла 128 Мб
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFileAsync([Required] IFileRepository fileRepository)
        {
            if (!Request.Form.Files.Any()) return NotFound(new { UploadStatus = _localizer["NotFound"] });

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
                            success = _apRepository.SetPlatforms(fileRepository.Data, _encoding);

                            // Очищаем свойства файлового репозитория
                            await fileRepository.ClearAsync();
                        }
                    }
                    catch (Exception ex) // Даже при реализованной глобальной обработке ошибок не убираем этот блок,
                                         // на случай того, что следующий файл будет валидным
                    {
                        _logger.LogError("{ex.ToString()}", ex.ToString());
                    }
                }
            }

            if (success) return Accepted(new { UploadStatus = _localizer["Accepted"] + $" {_apRepository.Count}." });
            return BadRequest(new { UploadStatus = _localizer["BadRequest"] });
        }

        /// <summary>
        /// Возврат всех рекламных площадок (без обработки, as is)
        /// </summary>
        /// <remarks>URL: /Api/{Culture}/APlatforms/GetAllPlatforms</remarks>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="500">Внутренняя ошибка сервера, в случае возникновения исключения</response>
        [HttpGet("GetAllPlatforms")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                    _localizer["FileNotFoundException2_P1"] + $"{ ControllerContext.ActionDescriptor.ControllerName}" + _localizer["FileNotFoundException2_P2"]);
            }
        }

        /// <summary>
        /// Метод поиска списка рекламных площадок для заданной локации
        /// </summary>
        /// <remarks>URL: /Api/{Culture}/APlatforms/SearchPlatforms
        /// 
        /// GET /SearchPlatforms
        /// {
        ///   "Location" : "/ru"
        /// }
        ///     
        /// </remarks>
        /// <param name="location">Параметр запроса, вида: /ru/svrd</param>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">В случае ошибки валидации запроса</response>
        /// <response code="500">Внутренняя ошибка сервера, в случае возникновения исключения</response>
        [HttpGet("SearchPlatforms")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IEnumerable<string> SearchPlatforms([FromQuery] string location)
        {
            if (QueryHelper.LocationValidator(ref location)) // Проводим валидацию строки запроса
            {
                if (!_apRepository.IsEmpty)
                {
                    foreach (var line in _apRepository.GetPlatforms(location))
                        yield return line;
                }
                else
                {
                    throw new FileNotFoundException(
                        _localizer["FileNotFoundException2_P1"] + $"{ControllerContext.ActionDescriptor.ControllerName}" + _localizer["FileNotFoundException2_P2"]);
                }
            }
            else throw new ArgumentException(_localizer["ArgumentException"]);
        }
    }
}