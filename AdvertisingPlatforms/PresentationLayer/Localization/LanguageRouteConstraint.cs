namespace AdvertisingPlatforms.PresentationLayer.Localization
{
    // Определяет контракт, который должен реализовать класс, чтобы проверить, является ли
    // значение параметра URL допустимым для ограничения
    internal class LanguageRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Определяет, содержит ли параметр URL допустимое значение для данного ограничения
        /// </summary>
        /// <param name="httpContext">Объект, инкапсулирующий информацию о HTTP-запросе</param>
        /// <param name="route">Маршрутизатор, к которому принадлежит это ограничение</param>
        /// <param name="routeKey">Имя проверяемого параметра</param>
        /// <param name="values">Словарь, содержащий параметры для URL</param>
        /// <param name="routeDirection">Объект, указывающий, выполняется ли проверка ограничения 
        /// при обработке входящего запроса или при генерации URL</param>
        /// <returns>true, если параметр URL содержит допустимое значение, в противном случае false</returns>
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            if (!values.ContainsKey("culture"))
                return false;

            var culture = values["culture"]?.ToString();
            return culture == "en-US" || culture == "ru-RU";
        }
    }
}