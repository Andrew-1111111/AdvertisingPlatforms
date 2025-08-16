using System.Threading.RateLimiting;
using AdvertisingPlatforms.BusinessLogic;
using AdvertisingPlatforms.Middlewares;

// Возможность расширить (в будущем) пропускную способность ThreadPool (выставлено значение: по умолчанию)
const int ratio = 1;
ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
ThreadPool.SetMaxThreads(workerThreads * ratio, completionPortThreads * ratio);

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
builder.Services.AddRepositories();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

var app = builder.Build();

// Вызов для включения ограничения скорости.
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

// Подключаем промежуточное ПО для глобальной обработки ошибок
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();