using Microsoft.AspNetCore.Localization;

namespace AdvertisingPlatforms.PresentationLayer.Localization
{
    // Поставщик абстрактного базового класса для определения информации о культуре Microsoft.AspNetCore.Http.HttpRequest
    internal class RouteDataRequestCultureProvider : RequestCultureProvider
    {
        /// <summary>
        /// Индекс культуры в запросе
        /// </summary>
        public int IndexOfCulture { get; set; }

        /// <summary>
        /// Индекс UI культуры в запросе
        /// </summary>
        public int IndexofUICulture { get; set; }

        /// <summary>
        /// Реализует провайдера для определения культуры данного запроса
        /// </summary>
        /// <param name="httpContext">Объект, инкапсулирующий информацию о HTTP-запросе</param>
        /// <returns>Определенный ProviderCultureResult. Возвращает null, если поставщик не смог определить ProviderCultureResult</returns>
        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            string? culture, uiCulture;
            culture = uiCulture = httpContext.Request.Path.Value?
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[IndexOfCulture]?.ToString();

            if (culture == null)
                return Task.FromResult<ProviderCultureResult?>(null);

            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, uiCulture));
        }
    }
}