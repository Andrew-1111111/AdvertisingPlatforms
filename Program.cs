using AdvertisingPlatforms.BusinessLogic;
using AdvertisingPlatforms.Middlewares;

// Расширяем пропускную способность ThreadPool
const int ratio = 2;
int workerThreads, completionPortThreads;
ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
ThreadPool.SetMaxThreads(workerThreads * ratio, completionPortThreads * ratio);

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
builder.Services.AddRepositories();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();