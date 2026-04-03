using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;
using WebApplication1.Data;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// === PostgreSQL ===
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=sportclubdb;Username=postgres;Password=secret123";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// === Redis ===
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION")
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConn));

// === Важно! Регистрация контроллеров ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();   // для Prometheus

var app = builder.Build();

// === Миграции + Seeding ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        db.Database.Migrate();
        Console.WriteLine("✅ Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
    }

    try
    {
        if (!db.Members.Any())
        {
            var member = new Member
            {
                FullName = "Александр Иванов",
                DateOfBirth = new DateTime(1997, 5, 15),
                Phone = "+7-999-123-4567",
                Email = "ivanov@example.com"
            };

            db.Members.Add(member);

            db.Subscriptions.Add(new Subscription
            {
                Member = member,
                Type = "Годовой безлимит",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                Price = 45000m
            });

            db.Trainings.Add(new Training
            {
                Member = member,
                DateTime = DateTime.UtcNow.AddDays(3),
                TrainerName = "Петров А.В.",
                ActivityType = "Кроссфит",
                DurationMinutes = 75
            });

            await db.SaveChangesAsync();
            Console.WriteLine("✅ Seeding data completed.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seeding error: {ex.Message}");
    }
}

// === Swagger и метрики ===
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseHttpMetrics();
app.MapMetrics();

app.Run();