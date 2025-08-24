# Advertising Platforms
Проект тестового задания (C# 9, WebAPI). Состоит из двух методов.

1. Метод загрузки рекламных площадок из файла (полностью перезаписывает всю хранимую информацию).
2. Метод поиска списка рекламных площадок для заданной локации.

## Структура кода
AdvertisingPlatforms
 - PresentationLayer - загрузка файла и получение площадок из GET запроса с локацией.
 - BusinessLogic - парсинг и обработка данных из файла.
 - FileStorage - хранение формы для загрузки файла и тестовый файл с названием TestCaseFile.txt, кодировка файла UTF-8 (без BOM).
 - Resources - ресурсы для локализации приложения.
 
AdvertisingPlatforms.Tests
 - Unit - тесты xUnit.

## Поддерживаемые языки
 - ru-RU
 - en-US

## Пути (и методы) запросов к API

|Основные методы из ТЗ|Описание метода|
|:-|:-|
| https://localhost:7201/Api/en-US/APlatforms/UploadFile | метод загрузки рекламных площадок из файла |
| https://localhost:7201/Api/en-US/APlatforms/SearchPlatforms | метод поиска списка рекламных площадок через GET запрос, формат запроса: SearchPlatforms?Location=/ru |

|Вспомогательные методы (для удобства отладки)|Описание метода|
|:-|:-|
| https://localhost:7201/Api/en-US/APlatforms | выводит простейшую форму для загрузки файла, форма загружает файл в /Api/en-US/APlatforms/UploadFile |
| https://localhost:7201/Api/en-US/APlatforms/GetAllPlatforms | возврат всех рекламных площадок (без обработки, as is) |

## Swagger
 https://localhost:7201/swagger/index.html

## Как запустить
Проект создан в VisualStudio 2022. Компилируем проект и запускаем, в браузере все запросы идут через конфигурацию HTTPS по адресу: https://localhost:7201/