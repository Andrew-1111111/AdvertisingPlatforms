using AdvertisingPlatforms.BusinessLogic.APlatforms;
using AdvertisingPlatforms.BusinessLogic.APlatforms.Interfaces;
using AdvertisingPlatforms.BusinessLogic.File;
using AdvertisingPlatforms.BusinessLogic.File.Interfaces;

namespace AdvertisingPlatforms.BusinessLogic
{
    /// <summary>
    /// Класс расширения, для подключения DI сервисов
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Метод расширения для подключения DI репозиториев
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection определяет контракт для коллекции дескрипторов служб</param>
        /// <returns>IServiceCollection определяет контракт для коллекции дескрипторов служб</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
        {
            // Добавляем репозиторий для работы с файлом
            serviceCollection.AddSingleton<IFileRepository, FileRepository>();               // Создаем единственный экземпляр сервиса

            // Добавляем репозиторий для работы с торговыми площадками (установка, выборка)
            serviceCollection.AddSingleton<IApPlatformsRepository, ApPlatformsRepository>(); // Создаем единственный экземпляр сервиса

            return serviceCollection;
        }
    }
}