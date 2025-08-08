namespace AdvertisingPlatforms.BusinessLogic.File
{
    public static class Extensions
    {
        /// <summary>
        /// Метод расширения для подключения DI репозиториев
        /// </summary>
        /// <param name="serviceCollection">this IServiceCollection определяет контракт для коллекции дескрипторов служб</param>
        /// <returns>IServiceCollection определяет контракт для коллекции дескрипторов служб</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
        {
            // Добавляем File репозиторий
            serviceCollection.AddSingleton<IFileRepository>(f => new File()); // Создаем единственный экземпляр сервиса через фабрику

            return serviceCollection;
        }
    }
}