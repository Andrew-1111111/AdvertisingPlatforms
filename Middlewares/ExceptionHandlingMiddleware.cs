using AdvertisingPlatforms.PresentationLayer.Dto;
using System.Net;

namespace AdvertisingPlatforms.Middlewares
{
    // Глобальный обработчик исключений
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private const bool INCLUDE_CALL_STACK = false;
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        /// <summary>
        /// Метод InvokeAsync (или Invoke) используется для вызова следующего делегата в конвейере Middleware
        /// </summary>
        /// <param name="context">HttpContext инкапсулирует всю специфичную для HTTP информацию об отдельном HTTP-запросе</param>
        /// <returns>Task представляет собой асинхронную операцию</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Передаем запрос следующему компоненту Middleware в конвейере
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex.Message, ex.StackTrace, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Логируем полученную ошибку и возвращаем ее статус код с описанием в JSON ответе
        /// </summary>
        /// <param name="context">HttpContext инкапсулирует всю специфичную для HTTP информацию об отдельном HTTP-запросе</param>
        /// <param name="exMessage">Строковое описание возникшего исключения</param>
        /// <param name="exCallStack">Строковой стек вызовов</param>
        /// <param name="statusCode">Содержит значения кодов состояния, определенных в RFC 2616 для протокола HTTP 1.1</param>
        /// <returns>Task представляет собой асинхронную операцию</returns>
        private async Task HandleExceptionAsync(HttpContext context, string exMessage, string? exCallStack, HttpStatusCode statusCode)
        {
            // Логируем полученную ошибку
            var loggerMsg = INCLUDE_CALL_STACK ? exMessage + Environment.NewLine + exCallStack : exMessage;
            _logger.LogError("{loggerMsg}", loggerMsg);

            // Формируем заголовки JSON ответа
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;

            // Формируем сообщение с текстом исключения, статус кодом и стеком вызовов (поле INCLUDE_CALL_STACK вкл/откл его отображение)
            var exDescrDto = new ExDescriptionDto(){ StatusCode = (int)statusCode, Exception = exMessage, CallStack = INCLUDE_CALL_STACK ? exCallStack : null };

            // Отправляем ответ в JSON формате (внутри вызываемого метода объект сериализуется и возвращается в UTF-8 кодировке)
            await response.WriteAsJsonAsync(exDescrDto);
        }
    }
}