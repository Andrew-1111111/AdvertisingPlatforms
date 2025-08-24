using System.Text;

namespace AdvertisingPlatforms.BusinessLogic.APlatforms.Interfaces
{
    /// <summary>
    /// Интерфейс, определяющий основной репозиторий с бизнес логикой 
    /// </summary>
    public interface IApPlatformsRepository
    {
        /// <summary>
        /// Возвращает значение, указывающее, является ли объект пустым
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Получает число элементов, содержащихся в репозитории
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Заполняет словарь с площадками
        /// </summary>
        /// <param name="data">Массив байт из строки в UTF-8 кодировке</param>
        /// <param name="encoding">UTF-8</param>
        /// <returns>Bool флаг успешности заполнения словаря</returns>
        public bool SetPlatforms(byte[] data, Encoding encoding);

        /// <summary>
        /// Получает платформы по выбранной локации
        /// </summary>
        /// <param name="location">Локация (пример: /ru/msk)</param>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        IEnumerable<string> GetPlatforms(string location);

        /// <summary>
        /// Возвращает полный список локаций с платформами 
        /// </summary>
        /// <returns>IEnumerable возвращает строковое перечисление через yield return, позволяет получать результаты в реальном времени</returns>
        IEnumerable<string> GetAllPlatforms();
    }
}