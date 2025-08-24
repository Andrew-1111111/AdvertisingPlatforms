namespace AdvertisingPlatforms.BusinessLogic.File.Interfaces
{
    /// <summary>
    /// Интерфейс, определяющий файловый репозиторий
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Расширение имени файла
        /// </summary>
        string Extension { get; set; }

        /// <summary>
        /// Флаг, обозначающий, загружен ли файл в репозиторий
        /// </summary>
        bool IsUploaded { get; set; }

        /// <summary>
        /// Массив байт из файла, загруженного в репозиторий
        /// </summary>
        byte[] Data { get; set; }

        /// <summary>
        /// Проводит валидацию и устанавливает IFormFile в свойства репозитория
        /// </summary>
        /// <param name="file">IFormFile представляет файл, отправленный с помощью HttpRequest</param>
        /// <returns>Task представляет собой асинхронную операцию</returns>
        Task<bool> SetFileAsync(IFormFile file);

        /// <summary>
        /// Выполняет очистку свойств репозитория
        /// </summary>
        Task ClearAsync();
    }
}