using System.Text.Json.Serialization;

namespace AdvertisingPlatforms.PresentationLayer.Dto
{
    // Структура для описания возникших исключений.
    // DTO (Data Transfer Object) - это шаблон проектирования, который используется для передачи данных между слоями приложения,
    // содержит минималистичное описание полей/свойств и практически не содержит кода.
    internal struct ExDescriptionDto
    {
        /// <summary>
        /// Конструктор структуры для описания возникших исключений
        /// </summary>
        public ExDescriptionDto()
        {
        }

        /// <summary>
        /// HTTP статус код
        /// </summary>
        [JsonPropertyOrder(-3)]
        [JsonPropertyName("HTTP status code")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Текст исключения
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyOrder(-2)]
        [JsonPropertyName("Exception text")]
        public string Exception { get; set; } = string.Empty;

        /// <summary>
        /// Стек вызовов
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyOrder(-1)]
        [JsonPropertyName("Exception call stack")]
        public string? CallStack { get; set; } = string.Empty;
    }
}