using AdvertisingPlatforms.BusinessLogic;
using AdvertisingPlatforms.PresentationLayer.Localization;
using AdvertisingPlatforms.PresentationLayer.Middlewares;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Threading.RateLimiting;

// Возможность расширить (в будущем) пропускную способность ThreadPool (выставлено значение: по умолчанию)
const int ratio = 1;
ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
ThreadPool.SetMaxThreads(workerThreads * ratio, completionPortThreads * ratio);

var builder = WebApplication.CreateBuilder(args);

// Регистрация локализации
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    CultureInfo[] supportedCultures = [new CultureInfo("en-US"), new CultureInfo("ru-RU")];

    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Индекс культуры в запросе. Пример: /Api/en-US/etc... где en-US будет под индексом 2.
    options.RequestCultureProviders = [new RouteDataRequestCultureProvider { IndexOfCulture = 2, IndexofUICulture = 2 }]; 
});

// Проверка, является ли значение параметра URL допустимым для ограничения
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("culture", typeof(LanguageRouteConstraint));
});

// Вводим Rate Limit для защиты от перегрузки по колл-ву запросов
builder.Services.AddRateLimiter(options =>
{
    // HTTP код ошибки, которая возвращается клиенту при превышении колл-ва запросов
    options.RejectionStatusCode = 429;

    // Глобальное ограничение по запросам применяется для каждого IP адреса
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Request.HttpContext.Connection.RemoteIpAddress!.ToString(),
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,           // FixedWindowRateLimiter будем автоматически обновлять счетчики
                PermitLimit = 100,                  // Максимум колл-во запросов
                QueueLimit = 0,                     // Отключаем механизм очередей (если он включен, API не выдает HTTP ошибку,
                                                    // запросы ожидают находясь в очередях)
                Window = TimeSpan.FromSeconds(15)   // Таймаут, на который блокируется подключение
            }));
});

// Регистрация сервисов
builder.Services.AddRepositories();

// Add services to the container.
builder.Services.AddControllers();

// Добавляем Swagger
builder.Services.AddSwaggerGen(options =>
{
    // Добавляем XML комментарии
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "AdvertisingPlatforms.xml"));

    // Заполняем шапку SwaggerUI
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Advertising Platforms",
        Description = "Проект тестового задания (C# 9, WebAPI)",
        Contact = new OpenApiContact
        {
            Name = "Github",
            Url = new Uri("https://github.com/Andrew-1111111/AdvertisingPlatforms")
        }
    });
});

var app = builder.Build();

// Подключаем промежуточное ПО для глобальной обработки ошибок
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Вызов для включения ограничения скорости.
app.UseRateLimiter();

// Подключаем промежуточное ПО для локализации
app.UseRequestLocalization(app.Services.GetService<IOptions<RequestLocalizationOptions>>()!.Value);

// Подключаем Swagger и UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();