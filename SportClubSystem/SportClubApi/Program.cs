using Microsoft.EntityFrameworkCore;
using Prometheus;
using SportClubApi.Data;

var builder = WebApplication.CreateBuilder(args);

// === Подключение к PostgreSQL ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=db;Port=5432;Database=sportclub;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// === Применение миграций и Seeding с повторными попытками ===
// Применение миграций и seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("🚀 Применяем миграции...");
        db.Database.Migrate();
        Console.WriteLine("✅ Миграции успешно применены.");

        // Временно отключаем seeding
        
        DbSeeder.Seed(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при инициализации базы: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpMetrics();
app.MapMetrics();

app.MapControllers();

app.Run();
