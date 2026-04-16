using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;
using SportClubApi.Data;
using SportClubApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Подключение к PostgreSQL
var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Database=sportclub;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connStr));

// Подключение Redis
// Подключение Redis с отключением AbortOnConnectFail (чтобы не падало при запуске без Docker)
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
var redisOptions = ConfigurationOptions.Parse(redisConn);
redisOptions.AbortOnConnectFail = false;   // ← важно!

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
builder.Services.AddScoped<CacheService>();

// Добавляем контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Автоматическое применение миграций и seeding данных
// Автоматическое применение миграций и seeding данных (с обработкой ошибок)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        db.Database.Migrate();
        DbSeeder.Seed(db);
        Console.WriteLine("✅ База данных успешно мигрирована и засеяна.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Не удалось подключиться к базе данных: {ex.Message}");
        Console.WriteLine("   Это нормально, если ты запускаешь без Docker. Продолжаем запуск...");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpMetrics();
app.MapMetrics();

app.UseAuthorization();
app.MapControllers();

app.Run();