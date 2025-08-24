using AdvertisingPlatforms.BusinessLogic.File.Interfaces;

namespace AdvertisingPlatforms.BusinessLogic.File
{
    /// <summary>
    /// Файловый репозиторий
    /// </summary>
    public class FileRepository : IFileRepository
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Расширение имени файла
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, обозначающий, загружен ли файл в репозиторий
        /// </summary>
        public bool IsUploaded { get; set; }

        /// <summary>
        /// Массив байт из файла, загруженного в репозиторий
        /// </summary>
        public byte[] Data { get; set; } = [0];

        private const string FILE_EXTENSION = ".txt";
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1); // Асинхронный аналог lock (мьютекс),
                                                                          // с текущим конструктором только один поток может получить доступ

        /// <summary>
        /// Проводит валидацию и устанавливает IFormFile в свойства репозитория
        /// </summary>
        /// <param name="file">IFormFile представляет файл, отправленный с помощью HttpRequest</param>
        /// <returns>Task представляет собой асинхронную операцию</returns>
        public async Task<bool> SetFileAsync(IFormFile file)
        {
            if (Validation(file))
            {
                // Считываем файл из IFromFile в MemoryStream
                await using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                // Заполняем временный буфер
                var buffer = ms.ToArray();

                if (buffer.Length > 0)
                {
                    // Асинхронно ожидаем получения семафора
                    await _semaphoreSlim.WaitAsync();

                    // Заполняем свойства класса
                    Name = Guid.NewGuid().ToString();
                    Extension = FILE_EXTENSION;
                    Data = buffer;
                    IsUploaded = true;

                    // Освобождаем семафор
                    _semaphoreSlim.Release();
                }
            }

            return IsUploaded;
        }

        /// <summary>
        /// Выполняет очистку свойств репозитория
        /// </summary>
        public async Task ClearAsync()
        {
            // Асинхронно ожидаем получения семафора
            await _semaphoreSlim.WaitAsync();

            Name = string.Empty;
            Extension = string.Empty;
            IsUploaded = false;
            Data = [0];

            // Освобождаем семафор
            _semaphoreSlim.Release();
        }

        /// <summary>
        /// Валидация файла (на пустоту файла, невалидные символы или пустоту в имени файла, на расширение)
        /// </summary>
        /// <param name="file">IFormFile представляет файл, отправленный с помощью HttpRequest</param>
        /// <returns>Bool флаг успешности валидации</returns>
        public static bool Validation(IFormFile file)
        {
            return file.Length > 0 
                   && !string.IsNullOrWhiteSpace(file.FileName) 
                   && Path.GetExtension(file.FileName).Equals(FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)
                   && file.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

    }
}